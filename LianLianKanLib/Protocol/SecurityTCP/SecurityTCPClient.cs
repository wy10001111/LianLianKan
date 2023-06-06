using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol.SecurityTCP
{
    public class SecurityTCPClient : SecurityTCP
    {
        #region 属性与变量

        /// <summary>
        /// 服务器
        /// </summary>
        ClientEx _server;
        public Client Server => _server;

        /// <summary>
        /// 是否连接
        /// </summary>
        public override bool IsConnected => _server?.Client.Connected ?? false;

        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event EventHandler<Client> LoseConnectionEventHandler;

        #endregion

        #region 方法

        /// <summary>
        /// 上报连接断开
        /// </summary>
        private void PostLoseConnection(ClientEx client) => LoseConnectionEventHandler?.Invoke(this, client);

        /// <summary>
        /// 连接服务器
        /// </summary>
        public bool StartConnectAsync(string host, int port)
        {
            if (this._server != null)
            {
                this.PushErrorEvent(_server, "You must STOP serve before START.");
                return false;
            }

            var autoResetEvent = new AutoResetEvent(false);
            //创建TCPServer
            this._server = new ClientEx(new TcpClient());
            this._server.SetAddress(host, port);
            //
            _server.Task = new Task(async () =>
            {
                try
                {
                    //连接
                    _server.Client.Connect(host, port);
                    //
                    using (this._encrytper = new Encrytper())
                    using (_server.Client)
                    using (_server.Stream = _server.Client.GetStream())
                    using (_server.TokenSource.Token.Register(() => _server.Stream.Close()))
                    {
                        var stream = _server.Stream;
                        //交换密钥
                        _server.PubKey = await WaitForKey(stream, this._encrytper.CrypPubKey.Length, _server.TokenSource.Token);
                        await this.SendKey(stream, this._encrytper.CrypPubKey, _server.TokenSource.Token);

                        //将连接成功的消息反馈
                        autoResetEvent.Set();

                        //开始接收数据
                        await Receiving(_server);

                        //上报丢失连接
                        PostLoseConnection(_server);
                    }
                }//try
                catch (ObjectDisposedException ex) { ex.GetType(); }
                catch (Exception ex)
                {
                    this.PushErrorEvent(IsConnected ? _server : null, ex.Message);
                }
                finally
                {
                    this._server = null;
                    autoResetEvent.Set();
                    autoResetEvent.Dispose();
                }
            }, TaskCreationOptions.LongRunning);
            _server.Task.Start();
            //等待连接
            if (autoResetEvent.WaitOne(1500))
                return true;
            //连接失败
            _server.TokenSource.Cancel();
            return false;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public bool StopConnect()
        {
            if (_server == null)
                return true;
            _server.TokenSource.Cancel();
            if (false == _server.Task.Wait(1500))
            {
                this.PushErrorEvent(_server, $"Failed to Stop listen client {_server.IP}::{_server.Port}");
                return false;
            }
            return true;
        }

        #endregion
    }

}
