using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace LianLianKanLib
{
    public class SecurityTCPServer : SecurityTCP
    {
        private class TCPServer
        {
            public TcpListener _listener;
            public bool _serveFlag;
            public Task _serveTask;
            public AutoResetEvent _syncEvent;
        }
        TCPServer _server;
        public override bool IsConnected => _server != null;
        private List<ClientEx> _clientList;
        public event EventHandler<Client> AcceptClientEvent;
        public event EventHandler<Client> MissClientEvent;
        private void PushNewClient(ClientEx client)
        {
            _clientList.Add(client);
            AcceptClientEvent?.Invoke(this, client);
        }
        private void MissClient(ClientEx client)
        {
            _clientList.Remove(client);
            MissClientEvent?.Invoke(this, client);
        }

        public bool StopServe()
        {
            if (this._server == null)
                return true;
            //只需要设置ServeFlag为true，然后就等待关闭即可。
            this._server._serveFlag = false;
            this._server?._syncEvent.WaitOne(1500);
            if (null != this._server)
            {
                this.PushErrorEvent(null, "Can't stop serving.");
                return false;
            }
            return true;
        }
        public bool StartServe(string ip, int port)
        {
            if (this._server != null)
            {
                this.PushErrorEvent(null, "You must STOP serve before START.");
                return false;
            }
            bool result = false;
            this._server = new TCPServer();
            this._server._syncEvent = new AutoResetEvent(false);
            this._server._serveTask = new Task(async () => {
                try
                {
                    IPAddress address;
                    if (false == IPAddress.TryParse(ip, out address))
                        address = IPAddress.Any;
                    using (this._encrytper = new Encrytper())
                    {
                        //
                        this._server._listener = new TcpListener(address, port);
                        this._server._listener.Start();
                        //打开成功，设置信号
                        this._server._serveFlag = true;
                        this._clientList = new List<ClientEx>();
                        result = true;
                        this._server._syncEvent.Set();
                        //监测新Client
                        while (this._server._serveFlag)
                        {
                            if (_server._listener.Pending())
                            {
                                var c = await _server._listener.AcceptTcpClientAsync();
                                if (c != null)
                                {
                                    ClientEx client = new ClientEx(c);
                                    this.StartListenClient(client);
                                }
                            }
                            else
                                await Task.Delay(10);
                        }
                        //该关闭服务了
                        this._server._listener.Stop();
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
                    var syncEvent = this._server._syncEvent;
                    this._server = null;
                    syncEvent.Set();
                    syncEvent.Dispose();
                }
            }, TaskCreationOptions.LongRunning);
            this._server._serveTask.Start();
            //等待结果
            this._server?._syncEvent.WaitOne(1500);
            return result;
        }
        public bool Disconnect(Client client)
        {
            return this.StopListenClient(client as ClientEx);
        }
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
                        //Send key
                        await stream.WriteAsync(this._encrytper.CrypPubKey, 0
                            , this._encrytper.CrypPubKey.Length, client.TokenSource.Token);
                        await stream.FlushAsync(client.TokenSource.Token);
                        //Wait for key
                        byte[] readBuff = new byte[StandardBuffLen];
                        int readSize = await stream.ReadAsync(readBuff, 0, readBuff.Length, client.TokenSource.Token);
                        if (readSize != this._encrytper.CrypPubKey.Length)
                            throw new Exception("Failed to exchange key.");
                        client.PubKey = new byte[readSize];
                        Array.Copy(readBuff, client.PubKey, readSize);
                        try
                        {
                            //连接成功的信号
                            signal.Set();
                            //上报
                            PushNewClient(client);
                            while (client.TokenSource.Token.IsCancellationRequested == false && readSize > 0)
                            {
                                readSize = await stream.ReadAsync(readBuff, 0, readBuff.Length, client.TokenSource.Token);
                                if (readSize > 0)
                                {
                                    byte[] data;
                                    int dataSize = this._encrytper.DecrytpData(client.PubKey, readBuff, 0, readSize, out data);
                                    Message msgEx = new Message(client, data, ref dataSize);
                                    this.PushMessageEvent(msgEx);
                                }
                            }
                        }
                        catch (Exception ex) { ex.GetType(); }
                        finally
                        {
                            //任何异常，都被视为连接成功后，断开连接。
                            //上报掉线
                            MissClient(client);
                        }
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

    }

    public class SecurityTCPClient : SecurityTCP
    {
        private class TCPServer : ClientEx
        {
            public TCPServer(string host, int port) : base(new TcpClient())
            {
                this.IP = host;
                this.Port = port;
            }
        }
        TCPServer _server;
        public Client Server => _server;
        public override bool IsConnected => _server?.Client.Connected ?? false;

        public event EventHandler<Client> LoseConnectionEvent;
        private void PostLoseConnection(ClientEx client) => LoseConnectionEvent?.Invoke(this, client);

        public bool StartConnectAsync(string host, int port)
        {
            if (this._server != null)
            {
                this.PushErrorEvent(_server, "You must STOP serve before START.");
                return false;
            }
            var autoResetEvent = new AutoResetEvent(false);
            this._server = new TCPServer(host, port);
            _server.Task = new Task(async () =>
            {
                try
                {
                    _server.Client.Connect(host, port);
                    using (this._encrytper = new Encrytper())
                    using (_server.Client)
                    using (_server.Stream = _server.Client.GetStream())
                    using (_server.TokenSource.Token.Register(() => _server.Stream.Close()))
                    {
                        var stream = _server.Stream;
                        //Wait for key
                        byte[] readBuff = new byte[StandardBuffLen];
                        int readSize = await stream.ReadAsync(readBuff, 0, readBuff.Length, _server.TokenSource.Token);
                        if (readSize != this._encrytper.CrypPubKey.Length)
                            throw new Exception("Failed to exchange key.");
                        _server.PubKey = new byte[readSize];
                        Array.Copy(readBuff, _server.PubKey, readSize);
                        //Send my key
                        await stream.WriteAsync(this._encrytper.CrypPubKey, 0
                            , this._encrytper.CrypPubKey.Length, _server.TokenSource.Token);
                        await stream.FlushAsync(_server.TokenSource.Token);
                        try
                        {
                            //将连接成功的消息反馈
                            autoResetEvent.Set();
                            //Start receiving message
                            while (_server.TokenSource.Token.IsCancellationRequested == false && readSize > 0)
                            {
                                readSize = await stream.ReadAsync(readBuff, 0, readBuff.Length, _server.TokenSource.Token);
                                if (readSize > 0)
                                {
                                    byte[] data;
                                    int dataSize = this._encrytper.DecrytpData(_server.PubKey, readBuff, 0, readSize, out data);
                                    Message msgEx = new Message(_server, data, ref dataSize);
                                    this.PushMessageEvent(msgEx);
                                }
                            }
                        }
                        catch (Exception ex) { ex.GetType(); }
                        finally
                        {
                            //任何异常，都被视为连接成功后，断开连接。
                            //上报
                            PostLoseConnection(_server);
                        }
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
            if (autoResetEvent.WaitOne(1500))
                return true;
            _server.TokenSource.Cancel();
            return false;
        }
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
        
    }

    public abstract class SecurityTCP
    {
        public abstract bool IsConnected { get; }
        protected class Encrytper : IDisposable
        {
            private CngKey _crypKey;
            public byte[] CrypPubKey { get; set; }
            public Encrytper()
            {
                this._crypKey = CngKey.Create(CngAlgorithm.ECDiffieHellmanP521);
                this.CrypPubKey = this._crypKey.Export(CngKeyBlobFormat.EccPublicBlob);
            }

            public int EncrytpData(byte[] pubKey, byte[] input, int offset, int inputSize, out byte[] output)
            {
                using (var keyAlg = new ECDiffieHellmanCng(this._crypKey))
                using (var otherKey = CngKey.Import(pubKey, CngKeyBlobFormat.EccPublicBlob))
                {
                    var symmKey = keyAlg.DeriveKeyMaterial(otherKey);
                    using (var aes = new AesCryptoServiceProvider())
                    {
                        aes.Key = symmKey;
                        aes.GenerateIV();
                        int ivSize = aes.IV.Count();
                        using (var encryptor = aes.CreateEncryptor())
                        using (var memStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                            {
                                memStream.Write(aes.IV, 0, ivSize);
                                cryptoStream.Write(input, offset, inputSize);
                            }
                            output = memStream.ToArray();
                            return output.Count();
                        }
                    }
                }
            }

            public int DecrytpData(byte[] pubKey, byte[] input, int offset, int inputSize, out byte[] output)
            {
                output = null;
                using (var KeyAlg = new ECDiffieHellmanCng(this._crypKey))
                using (var otherKey = CngKey.Import(pubKey, CngKeyBlobFormat.EccPublicBlob))
                {
                    var sysmmKey = KeyAlg.DeriveKeyMaterial(otherKey);
                    using (var aes = new AesCryptoServiceProvider())
                    {
                        int ivSize = aes.BlockSize >> 3;
                        if (inputSize < ivSize)
                            return 0;
                        int rawDataSize = inputSize - ivSize;
                        ArraySegment<byte> seg = new ArraySegment<byte>(input, offset, ivSize);
                        aes.IV = seg.ToArray();
                        aes.Key = sysmmKey;
                        using (var decryptor = aes.CreateDecryptor())
                        using (var memStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(input, ivSize + offset, rawDataSize);
                            }
                            output = memStream.ToArray();
                            return output.Length;
                        }
                    }
                }
            }

            public void Dispose()
            {
                this._crypKey.Dispose();
            }

        }
        protected Encrytper _encrytper;
        public class Message : EventArgs
        {
            public Message(Client client, byte[] msg, ref int size)
            {
                this.Client = client;
                this.Msg = msg;
                this.Size = size;
            }
            public Client Client { get; }
            public byte[] Msg { get; }
            public int Size { get; }
        }
        public class Error : EventArgs
        {
            public Error(Client client, ref string message)
            {
                this.Client = client;
                this.Message = message;
            }
            public Client Client { get; }
            public string Message { get; set; }
        }
        public event EventHandler<Message> MessageEvent;
        public event EventHandler<Error> ErrorEvent;
        protected void PushMessageEvent(Message msg) => this.MessageEvent?.Invoke(this, msg);
        protected void PushErrorEvent(Client client, string error) => this.ErrorEvent?.Invoke(this, new Error(client, ref error));

        public class Client : IComparable<Client>
        {
            public string IP { get; protected set; }
            public int Port { get; protected set; }

            public static bool operator ==(Client client1, Client client2)
            {
                if (Object.ReferenceEquals(client1, client2))
                    return true;
                if (Object.ReferenceEquals(client1, null) || Object.ReferenceEquals(client2, null))
                    return false;
                return client1.IP == client2.IP && client1.Port == client2.Port;
            }
            public static bool operator !=(Client client1, Client client2) => !(client1 == client2);
            public override bool Equals(object obj)
            {
                return this == (obj as Client);
            }
            public override int GetHashCode()
            {
                return (IP.GetHashCode()) | Port;
            }

            public override string ToString()
            {
                return $"{IP}:{Port}";
            }

            public int CompareTo(Client other)
            {
                if (this == null)
                    return -1;
                if (other == null)
                    return 1;
                return this.GetHashCode() - other.GetHashCode();
            }
        }
        protected class ClientEx : Client
        {
            public ClientEx(TcpClient client)
            {
                Client = client;
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 3000;
                IPEndPoint ipep = Client.Client.RemoteEndPoint as IPEndPoint;
                if (null != ipep)
                {
                    this.IP = ipep.Address.ToString();
                    this.Port = ipep.Port;
                }
                this.TokenSource = new CancellationTokenSource();
                //心跳包
                uint open = 1;    //开启/关闭 标志
                uint beginTime = 3000;  //闲时启动时间
                uint keepSpan = 1000;  //发送时间间隔
                uint dummy = 0;
                byte[] inValue = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes(open).CopyTo(inValue, 0); //
                BitConverter.GetBytes(beginTime).CopyTo(inValue, Marshal.SizeOf(open));
                BitConverter.GetBytes(keepSpan).CopyTo(inValue, Marshal.SizeOf(open) + Marshal.SizeOf(beginTime));
                Client.Client.IOControl(IOControlCode.KeepAliveValues, inValue, null);
            }
            public TcpClient Client { get; set; }
            public Task Task { get; set; }
            public NetworkStream Stream;
            public CancellationTokenSource TokenSource { get; set; }
            public byte[] PubKey { get; set; }
        }
        public static int StandardBuffLen = 1024 * 1024 * 6; //6M
        public static int StandardBuffLenMax = StandardBuffLen - 1024 * 69;

        public void Send(Client client, byte[] data, int dataSize)
        {
            ClientEx clientEx = client as ClientEx;
            if (clientEx == null)
                return;
            if (clientEx.TokenSource.Token.IsCancellationRequested)
                return;
            if (clientEx.Client.Connected == false)
                return;
            int offset = 0, encrytpSize = 0; ;
            while (dataSize > 0)
            {
                encrytpSize = dataSize > StandardBuffLenMax ? StandardBuffLenMax : dataSize;
                byte[] sendData;
                int sendSize = this._encrytper.EncrytpData(clientEx.PubKey, data, offset, encrytpSize, out sendData);
                clientEx.Stream.WriteAsync(sendData, 0, sendSize, clientEx.TokenSource.Token);
                clientEx.Stream.FlushAsync(clientEx.TokenSource.Token);
                offset += encrytpSize;
                dataSize -= encrytpSize;
            }
        }

    }

}


