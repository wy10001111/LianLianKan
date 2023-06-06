using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    public class SecurityTCPServer : SecurityTCP
    {
        #region 属性与变量

        /// <summary>
        /// TCP监听者
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// 服务标志
        /// </summary>
        private bool _serveFlag;

        /// <summary>
        /// 服务Task
        /// </summary>
        private Task _serveTask;

        /// <summary>
        /// 同步信号
        /// </summary>
        private AutoResetEvent _syncEvent;

        /// <summary>
        /// 是否开始连接
        /// </summary>
        public override bool IsConnected => this._serveFlag;

        /// <summary>
        /// Client列表
        /// </summary>
        private List<ClientEx> _clientList;

        /// <summary>
        /// 接受Client事件
        /// </summary>
        public event EventHandler<Client> AcceptClientEventHandler;

        /// <summary>
        /// Client失联事件
        /// </summary>
        public event EventHandler<Client> MissClientEventHandler;

        #endregion

        #region 方法

        /// <summary>
        /// 上报新Client
        /// </summary>
        private void PushNewClient(ClientEx client)
        {
            _clientList.Add(client);
            AcceptClientEventHandler?.Invoke(this, client);
        }

        /// <summary>
        /// 上报Client失联
        /// </summary>
        private void PushMissingClient(ClientEx client)
        {
            _clientList.Remove(client);
            MissClientEventHandler?.Invoke(this, client);
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public bool StopServe()
        {
            if (this._listener == null)
                return true;
            //只需要设置ServeFlag为true，然后就等待关闭即可。
            this._serveFlag = false;
            this._syncEvent.WaitOne(2000);
            if (this._listener != null)
            {
                this.PushErrorEvent(null, "Can't stop serving.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 开始服务
        /// </summary>
        public bool StartServe(string ip, int port)
        {
            if (this._listener != null)
            {
                this.PushErrorEvent(null, "You must STOP serve before START.");
                return false;
            }
            bool result = false;
            this._syncEvent = new AutoResetEvent(false);
            this._serveTask = new Task(async () => {
                try
                {
                    IPAddress address;
                    if (false == IPAddress.TryParse(ip, out address))
                        address = IPAddress.Any;
                    using (this._encrytper = new Encrytper())
                    {
                        //开启监听
                        this._listener = new TcpListener(address, port);
                        this._listener.Start();
                        //打开成功，设置信号
                        this._serveFlag = true;
                        this._clientList = new List<ClientEx>();
                        result = true;
                        this._syncEvent.Set();
                        //监测新Client
                        try
                        {
                            while (this._serveFlag)
                            {
                                //判断是否有连接请求
                                if (_listener.Pending())
                                {
                                    //接收Client
                                    var c = await _listener.AcceptTcpClientAsync();
                                    if (c != null)
                                    {
                                        ClientEx client = new ClientEx(c);
                                        //开始监听Client数据
                                        this.StartListenClient(client);
                                    }
                                }
                                else
                                    Task.Delay(10).Wait();
                            }
                        }
                        catch (Exception)
                        {

                        }
                        //该关闭服务了
                        this._listener.Stop();
                        while (this._clientList.Count() > 0)
                        {
                            this.StopListenClient(this._clientList.First());
                        }
                    }
                }
                catch (Exception ex)
                {
                    //异常
                    this.PushErrorEvent(null, ex.Message);
                }
                finally
                {
                    var syncEvent = this._syncEvent;
                    syncEvent.Set();
                    syncEvent.Dispose();
                    this._syncEvent = null;
                    this._listener = null;
                }
            }, TaskCreationOptions.LongRunning);
            this._serveTask.Start();
            //等待结果
            this._syncEvent.WaitOne(1500);
            return result;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public bool Disconnect(Client client)
        {
            return this.StopListenClient(client as ClientEx);
        }

        /// <summary>
        /// 停止监听Client
        /// </summary>
        private bool StopListenClient(ClientEx client)
        {
            client.TokenSource.Cancel();
            if (false == client.Task.Wait(1000))
            {
                this.PushErrorEvent(client, $"Failed to Stop listen client {client.IP}::{client.Port}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 开始监听Client
        /// </summary>
        private bool StartListenClient(ClientEx client)
        {
            var signal = new ManualResetEventSlim(false);
            client.Task = Task.Run(async () =>
            {
                //第一个try catch被认为，无法连接成功
                try
                {
                    using (client.Client)
                    using (client.Stream = client.Client.GetStream())
                    using (client.TokenSource.Token.Register(() => client.Stream.Close()))
                    {
                        var stream = client.Stream;
                        //交换密钥
                        await this.SendKey(stream, this._encrytper.CrypPubKey, client.TokenSource.Token);
                        client.PubKey = await WaitForKey(stream, this._encrytper.CrypPubKey.Length, client.TokenSource.Token);

                        //连接成功的信号
                        signal.Set();

                        //上报
                        PushNewClient(client);

                        //开始接收数据
                        await Receiving(client);

                        //上报掉线
                        PushMissingClient(client);
                    }
                }
                catch (ObjectDisposedException ex) { ex.GetType(); }
                catch (Exception ex)
                {
                    this.PushErrorEvent(client, ex.Message);
                }
                finally
                {
                    signal.Set();
                    signal.Dispose();
                }
            });
            if (signal.Wait(3000))
                return true;
            //等待不到，就自动关闭
            client.TokenSource.Cancel();
            return false;
        }

        #endregion

    }

}
