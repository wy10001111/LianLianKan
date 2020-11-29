using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace LianLianKanLib
{
    public class ProtocolError
    {
        public ProtocolError(UIUser user, string error)
        {
            this.User = user;
            this.Error = error;
        }
        public UIUser User { get; }
        public string Error { get; }
    }

    public abstract class Protocol : IDisposable
    {
        public Protocol()
        {
            RunPostor();
        }
        ~Protocol()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            DistroyPoster();
        }

        public bool IsConnected => _tcp.IsConnected;
        public Dictionary<SecurityTCP.Client, UIUser> UserList { get; } = new Dictionary<SecurityTCP.Client, UIUser>();
        protected void AddUser(SecurityTCP.Client client, UIUser user)
        {
            UserList.Add(client, user);
            BigMsgList.Add(client, new Queue<byte[]>());
            MsgList.Add(client, new Queue<Message>());
        }
        protected void RemoveUser(SecurityTCP.Client client)
        {
            UserList[client].Client = null;
            MsgList.Remove(client);
            BigMsgList.Remove(client);
            UserList.Remove(client);
        }
        protected void ClearUser()
        {
            while (UserList.Count > 0)
                RemoveUser(UserList.Keys.ElementAt(0));
        }
        private SecurityTCP _tcp;
        protected SecurityTCP TCP
        {
            get
            {
                return _tcp;
            }
            set
            {
                if (_tcp != value && _tcp != null)
                {
                    _tcp.ErrorEvent -= PushError;
                    _tcp.MessageEvent -= MessageArrived;
                }
                _tcp = value;
                _tcp.ErrorEvent += PushError;
                _tcp.MessageEvent += MessageArrived;
            }
        }
        public event EventHandler<ProtocolError> ErrorEvent;
        protected void PushError(object obj, SecurityTCP.Error e) => ErrorList.Enqueue(new ProtocolError(e.Client != null ? UserList[e.Client] : null, e.Message));
        protected void PushError(object obj, UIUser user, string e) => ErrorList.Enqueue(new ProtocolError(user, e));
        public event EventHandler<Message> MessageEvent;
        protected void PushMessge(Message msg) => MessageList.Enqueue(msg);
        protected ConcurrentQueue<ProtocolError> ErrorList = new ConcurrentQueue<ProtocolError>();
        protected ConcurrentQueue<Message> MessageList = new ConcurrentQueue<Message>();
        private bool postorFlag;
        private void DistroyPoster() => postorFlag = false;
        private void RunPostor()
        {
            postorFlag = true;
            Task.Run(async () => {
                ProtocolError error;
                Message msg;
                while (postorFlag)
                {
                    try
                    {
                        if (ErrorList.TryDequeue(out error))
                        {
                            ErrorEvent?.Invoke(this, error);
                        }
                        else if (MessageList.TryDequeue(out msg))
                        {
                            if (msg.MessageID == MessageID.MI_BIG_BOX)
                                this.UnpackBigBox(msg as MessageBigBox);
                            else
                                MessageEvent?.Invoke(this, msg);
                        }
                        else
                            await Task.Delay(1);
                    }
                    catch (Exception ex) { ex.GetType(); }
                }
                //退出，清理所有消息
                while (ErrorList.TryDequeue(out error))
                {
                    ErrorEvent?.Invoke(this, error);
                }
                while (MessageList.TryDequeue(out msg))
                {
                    //MessageEvent?.Invoke(this, msg);
                }
            });
        } 

        public readonly int  CommandSize = 6;
        protected class Command
        {
            public Command(string symbol)
            {
                Symbol = symbol;
                Data = Encoding.Unicode.GetBytes(symbol);
            }
            public readonly string Symbol;
            public readonly byte[] Data;
        }
        protected readonly Command[] Commands = {
            new Command("App"),   //Apply sending Permit
            new Command("All"),     //Allow sending Permit
            new Command("Ent"),    // Message Entity
            new Command("Flu"),    // Flush Connection
        };

        protected event EventHandler<Tuple<SecurityTCP.Client, string>> AllowEvent;
        private Dictionary<SecurityTCP.Client, Queue<Message>> MsgList { get; } = new Dictionary<SecurityTCP.Client, Queue<Message>>();
        private void MessageArrived(object sender, SecurityTCP.Message e)
        {
            string cmd = Encoding.Unicode.GetString(e.Msg, 0, CommandSize);
            if (Commands[0].Symbol == cmd)
                this.CommandOfApply(e.Msg, e.Size, e.Client);
            else if (Commands[1].Symbol == cmd)
                this.CommandOfAllow(e.Msg, e.Size, e.Client);
            else if (Commands[2].Symbol == cmd)
                this.CommandOfEntity(e.Msg, e.Size, e.Client);
            else if (Commands[3].Symbol == cmd)
                this.CommandOfFlush(e.Msg, e.Size, e.Client);
            else
            {
                //丢弃不符合协议的消息
            }
        }
        private void CommandOfApply(byte[] data, int size, SecurityTCP.Client client)
        {
            if (size - CommandSize > 128)
                return;
            string msgName = Encoding.Unicode.GetString(data, CommandSize, size - CommandSize);
            Message msgClass = Message.CreateInstance(msgName);
            if (msgClass != null)
            {
                msgClass.User = UserList[client];
                MsgList[client].Enqueue(msgClass);
                //允许发送
                this.SendMsg(client, Commands[1], msgClass.MsgName);
            }
        }
        private void CommandOfAllow(byte[] data, int size, SecurityTCP.Client client)
        {
            string msgStr = Encoding.Unicode.GetString(data, CommandSize, size - CommandSize);
            AllowEvent?.Invoke(this, new Tuple<SecurityTCP.Client, string>(client, msgStr));
        }
        private void CommandOfEntity(byte[] data, int size, SecurityTCP.Client client)
        {
            var queue = MsgList[client];
            while (queue.Count > 0)
            {
                var msgClass = queue.Dequeue();
                var realMsg = Message.SetMessageEntity(msgClass.MsgName, data, CommandSize, size - CommandSize);
                if (realMsg != null)
                {
                    realMsg.User = UserList[client];
                    this.PushMessge(realMsg);
                    break;
                }
            }
        }
        private void CommandOfFlush(byte[] data, int size, SecurityTCP.Client client)
        {
        }
        private Dictionary<SecurityTCP.Client, Queue<byte[]>> BigMsgList { get; } = new Dictionary<SecurityTCP.Client, Queue<byte[]>>();
        private void UnpackBigBox(MessageBigBox msg)
        {
            if (msg.User == null)
                return;
            var queue = BigMsgList[msg.User.Client];
            //判断是否为第一个Box
            if (msg.Offset == 0)
            {
                queue.Clear();
                queue.Enqueue(new byte[msg.TotalSize]);
            }
            //复制数据
            Array.Copy(msg.Data, 0, queue.Peek(), msg.Offset, msg.Data.Length);
            //判断是否发送完毕
            if (msg.Offset + msg.Data.Length == msg.TotalSize)
            {
                var data = queue.Dequeue();
                var realMsg = Message.SetMessageEntity(msg.InnnerMsgName, data, 0, data.Length);
                if (realMsg != null)
                {
                    realMsg.User = msg.User;
                    this.PushMessge(realMsg);
                }
            }
        }

        protected byte[] _sendBuff = new byte[1024 * 1024 * 2]; //1M
        public virtual void SendMsg(Message msg)
        {
            if (IsConnected == false)
                return;
            try
            {
                if (ApplySendedPermission(msg.User.Client, msg.MsgName))
                {
                    Commands[2].Data.CopyTo(_sendBuff, 0);
                    int size = CommandSize + msg.GetMessgeEntity(_sendBuff, CommandSize, _sendBuff.Length - CommandSize);
                    _tcp.Send(msg.User.Client, _sendBuff, size);
                }
                else
                    throw new Exception("No permission to sending !");
            }
            catch (Exception e)
            {
                e.GetType();
            }

        }
        public virtual void SendBigMsg(Message msg)
        {
            if (IsConnected == false)
                return;
            //先得到原始数据
            var bigBuff = new byte[SecurityTCP.StandardBuffLen];
            int size = msg.GetMessgeEntity(bigBuff, 0, bigBuff.Length);
            //初始化数据
            int limitSendSize = (int)(_sendBuff.Length * 0.69);
            var box = new MessageBigBox()
            {
                InnnerMsgName = "",
                User = msg.User,
                UserID = msg.UserID,
                CallID = 0,
                TotalSize = size,
                Offset = 0,
            };
            //分类发送
            if (size > limitSendSize)
            {
                box.Data = new byte[limitSendSize];
                do
                {
                    Array.Copy(bigBuff, box.Offset, box.Data, 0, limitSendSize);
                    this.SendMsg(box);
                    box.Offset += limitSendSize;
                    size -= limitSendSize;
                } while (size > limitSendSize);
            }
            //最后一包，必须包含名称
            box.InnnerMsgName = msg.MsgName;
            box.Data = new byte[size];
            Array.Copy(bigBuff, box.Offset, box.Data, 0, size);
            this.SendMsg(box);

        }
        private void SendMsg(SecurityTCP.Client client, Command cmd, string text)
        {
            cmd.Data.CopyTo(_sendBuff, 0);
            var data = Encoding.Unicode.GetBytes(text);
            data.CopyTo(_sendBuff, cmd.Data.Length);
            this.TCP.Send(client, _sendBuff, cmd.Data.Length + data.Length);
        }

        protected bool ApplySendedPermission(SecurityTCP.Client client, string msgName)
        {
            //请求发送
            var sendPermit = false;
            using (var signal = new ManualResetEventSlim(false))
            {
                EventHandler<Tuple<SecurityTCP.Client, string>> allowback = (object sender, Tuple<SecurityTCP.Client, string> pair) =>
                {
                    if (client == pair.Item1 && pair.Item2 == msgName)
                        signal.Set();
                };
                this.AllowEvent += allowback;
                sendPermit = false;
                    this.SendMsg(client, Commands[0], msgName);
                //等待允许
                if (signal.Wait(1500))
                {
                    //获得准许
                    sendPermit = true;
                }
                //this.PushError(this, UserList[client], $"No permission to sending from {user.Name} !");
                this.AllowEvent -= allowback;
            }
            return sendPermit;
        }
    }

    public class ServerProtocol : Protocol
    {
        public ServerProtocol()
        {
            var server = new SecurityTCPServer();
            server.AcceptClientEvent += AcceptUser;
            server.MissClientEvent += MissUser;
            this.TCP = server;
        }

        public void AcceptUser(object sender, SecurityTCP.Client client)
        {
            this.AddUser(client, new UIUser("null", "未知用户") { Client = client });
        }
        public void MissUser(object sender, SecurityTCP.Client client)
        {
            this.RemoveUser(client);
        }

        public bool StartServing(string ip, int port)
        {
            var server = this.TCP as SecurityTCPServer;
            return server.StartServe(ip, port);
        }

        public bool StopServing()
        {
            var server = this.TCP as SecurityTCPServer;
            return server.StopServe();
        }
        
    }

    public class ClientProtocol : Protocol
    {
        public ClientProtocol()
        {
            var client = new SecurityTCPClient();
            this.TCP = client;
        }

        public Action OfflineEvent;
        private void PostOffineEvent(object obj, SecurityTCP.Client c)
        {
            Task.Run(() =>
            {
                this.Disconnect();
                OfflineEvent?.Invoke();
            });
        }


        public bool Connect(string host, int port)
        {
            var client = this.TCP as SecurityTCPClient;
            if (client.StartConnectAsync(host, port))
            {
                client.LoseConnectionEvent += PostOffineEvent;
                this.AddUser(client.Server, new UIUser("null", "未知用户") { Client = client.Server });
            }
            return IsConnected;
        }

        public bool Disconnect()
        {
            var client = this.TCP as SecurityTCPClient;
            client.LoseConnectionEvent -= PostOffineEvent;
            if (client.StopConnect())
            {
                this.ClearUser();
            }
            return IsConnected != true;
        }
        
    }

}
