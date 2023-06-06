using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LianLianKanLib.Protocol
{
    public class Protocol
    {
        public Protocol()
        {
            Delivering();
        }

        ~Protocol()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            StopDelivering();
        }

        #region 属性与变量

        /// <summary>
        /// TCP服务
        /// </summary>
        private SecurityTCP.SecurityTCP _tcp;
        protected SecurityTCP.SecurityTCP TCP
        {
            get
            {
                return _tcp;
            }
            set
            {
                if (_tcp != value && _tcp != null)
                {
                    _tcp.ErrorEventHandler -= PushError;
                    _tcp.PackageEventHandler -= PackageArrived;
                }
                _tcp = value;
                _tcp.ErrorEventHandler += PushError;
                _tcp.PackageEventHandler += PackageArrived;
            }
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected => _tcp.IsConnected;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<ProtocolError> ErrorEventHandler;

        /// <summary>
        /// 消息事件
        /// </summary>
        public event EventHandler<Message> MessageEventHandler;

        /// <summary>
        /// Allow Command事件
        /// </summary>
        protected event EventHandler<Tuple<SecurityTCP.Client, string>> AllowEventHandler;

        /// <summary>
        /// 掉线事件 
        /// </summary>
        public Action<Endpoint> OfflineEventHandler;

        /// <summary>
        /// 错误列表
        /// </summary>
        protected ConcurrentQueue<ProtocolError> ErrorList = new ConcurrentQueue<ProtocolError>();

        /// <summary>
        /// 消息列表
        /// </summary>
        protected ConcurrentQueue<Message> MessageList = new ConcurrentQueue<Message>();

        /// <summary>
        /// Allowed Message队列事件
        /// </summary>
        private Dictionary<SecurityTCP.Client, Queue<Message>> AllowedMsgList { get; } 
            = new Dictionary<SecurityTCP.Client, Queue<Message>>();

        /// <summary>
        /// 大消息列表
        /// </summary>
        private Dictionary<Endpoint, Queue<byte[]>> BigMsgList { get; } 
            = new Dictionary<Endpoint, Queue<byte[]>>();

        /// <summary>
        /// 端点列表
        /// </summary>
        protected Dictionary<SecurityTCP.Client, Endpoint> EndpointList { get; } 
            = new Dictionary<SecurityTCP.Client, Endpoint>();

        /// <summary>
        /// 派送标志
        /// </summary>
        private bool _deliverFlag = false;

        /// <summary>
        /// 发送缓冲区
        /// </summary>
        protected byte[] _sendBuff = new byte[SecurityTCP.SecurityTCP.StandardBuffLen];

        #endregion

        #region 方法

        /// <summary>
        /// 添加端
        /// </summary>
        protected void AddEndpoint(Endpoint endpoint)
        {
            EndpointList.Add(endpoint.Client, endpoint);
            BigMsgList.Add(endpoint, new Queue<byte[]>());
            AllowedMsgList.Add(endpoint.Client, new Queue<Message>());
        }

        /// <summary>
        /// 移除端
        /// </summary>
        protected void RemoveEndpoint(Endpoint endpoint)
        {
            AllowedMsgList.Remove(endpoint.Client);
            BigMsgList.Remove(endpoint);
            EndpointList.Remove(endpoint.Client);
        }

        /// <summary>
        /// 清理所有端
        /// </summary>
        protected void ClearEndpoint()
        {
            while (EndpointList.Count > 0)
                RemoveEndpoint(EndpointList.Values.ElementAt(0));
        }

        /// <summary>
        /// 推送错误
        /// </summary>
        protected void PushError(object sender, SecurityTCP.Error e) 
            => ErrorList.Enqueue(new ProtocolError(e.Client != null ? EndpointList[e.Client] : null, e.Message));
        protected void PushError(object sender, Endpoint end, string e) => ErrorList.Enqueue(new ProtocolError(end, e));

        /// <summary>
        /// 推送消息
        /// </summary>
        protected void PushMessge(Message msg) => MessageList.Enqueue(msg);

        /// <summary>
        /// 停止派送
        /// </summary>
        protected void StopDelivering() => _deliverFlag = false;

        /// <summary>
        /// 开始派送
        /// </summary>
        private void Delivering()
        {
            _deliverFlag = true;
            Task.Run(() => {
                ProtocolError error;
                Message msg;
                while (_deliverFlag)
                {
                    try
                    {
                        if (ErrorList.TryDequeue(out error))
                        {
                            ErrorEventHandler?.Invoke(this, error);
                        }
                        else if (MessageList.TryDequeue(out msg))
                        {
                            if (msg.MessageID == MessageID.MI_BIG_BOX)
                                this.DeliveringBigBox(msg as MessageBigBox);
                            else
                                MessageEventHandler?.Invoke(this, msg);
                        }
                        else
                            Task.Delay(1).Wait();
                    }
                    catch (Exception ex) { ex.GetType(); }
                }
                //退出，清理所有消息
                while (ErrorList.TryDequeue(out error))
                {
                    ErrorEventHandler?.Invoke(this, error);
                }
                while (MessageList.TryDequeue(out msg))
                {
                    //MessageEvent?.Invoke(this, msg);
                }
            });
        }

        /// <summary>
        /// 派送大消息
        /// </summary>
        private void DeliveringBigBox(MessageBigBox msg)
        {
            if (msg.Endpoint == null)
                return;
            var queue = BigMsgList[msg.Endpoint];
            //判断是否为第一个Box
            if (msg.Offset == 0)
            {
                queue.Clear();
                queue.Enqueue(new byte[msg.TotalSize]);
            }
            //复制数据
            Array.Copy(msg.Data, 0, queue.Peek(), msg.Offset, msg.Data.Length);
            //判断是否接收完整
            if (msg.Offset + msg.Data.Length == msg.TotalSize)
            {
                var data = queue.Dequeue();
                //反序列化成消息
                var realMsg = Message.DeserializeMessage(msg.InnnerMsgName, data, 0, data.Length);
                if (realMsg != null)
                {
                    realMsg.Endpoint = msg.Endpoint;
                    //向上层推送
                    this.PushMessge(realMsg);
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public virtual void SendMsg(Message msg)
        {
            if (IsConnected == false)
                return;
            try
            {
                //请求发送消息
                if (ApplySendedPermission(msg.Endpoint.Client, msg.MsgName))
                {
                    var data = Message.SerializeMessage(msg);
                    this.SendCommand(msg.Endpoint.Client, CommandSymbol.Ent, data);
                }
                else
                    throw new Exception("No permission to send !");
            }
            catch (Exception e)
            {
                e.GetType();
            }

        }

        /// <summary>
        /// 发送命令
        /// </summary>
        private void SendCommand(SecurityTCP.Client client, CommandSymbol symbol, byte[] data)
        {
            var cmd = new Command()
            {
                Symbol = symbol,
                Data = data
            };
            var size = Command.SerializeCommand(cmd, _sendBuff, 0, _sendBuff.Length);
            this.TCP.Send(client, _sendBuff, size);
        }

        /// <summary>
        /// 发送大消息
        /// </summary>
        public virtual void SendBigMsg(Message msg)
        {
            if (IsConnected == false)
                return;
            //先得到原始数据
            var bigBuff = Message.SerializeMessage(msg);
            int size = bigBuff.Length;
            //初始化数据
            int limitSendSize = (int)(_sendBuff.Length * 0.1);
            var box = new MessageBigBox()
            {
                InnnerMsgName = "",
                Endpoint = msg.Endpoint,
                UserID = msg.UserID,
                CallID = 0,
                TotalSize = size,
                Offset = 0,
                Data = new byte[limitSendSize]
            };
            //分组发送
            if (size > limitSendSize)
            {
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

        /// <summary>
        /// 请求发送Command
        /// </summary>
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
                this.AllowEventHandler += allowback;
                sendPermit = false;
                //发送App命令
                this.SendCommand(client, CommandSymbol.App, Encoding.Unicode.GetBytes(msgName));
                //等待允许
                if (signal.Wait(1500))
                {
                    //获得准许
                    sendPermit = true;
                }
                //this.PushError(this, UserList[client], $"No permission to sending from {user.Name} !");
                this.AllowEventHandler -= allowback;
            }
            return sendPermit;
        }

        /// <summary>
        /// 接收数据包
        /// </summary>
        private void PackageArrived(object sender, SecurityTCP.TCPPackage e)
        {
            //序列化
            var cmd = Command.DeserializeCommand(e.Data, 0, e.Data.Length);
            //指定方法处理不同的命令
            if (CommandSymbol.App == cmd.Symbol)
                this.CommandApply(cmd, e.Client);
            else if (CommandSymbol.All == cmd.Symbol)
                this.CommandAllow(cmd, e.Client);
            else if (CommandSymbol.Ent == cmd.Symbol)
                this.CommandEntity(cmd, e.Client);
            else if (CommandSymbol.Flu == cmd.Symbol)
                this.CommandFlush(cmd, e.Client);
            else
            {
                //丢弃不符合协议的命令
            }
        }

        /// <summary>
        /// 处理请求发送命令
        /// </summary>
        private void CommandApply(Command cmd, SecurityTCP.Client client)
        {
            //获得消息名
            string msgName = Encoding.Unicode.GetString(cmd.Data, 0, cmd.Data.Length);
            //创建消息实例
            Message msgClass = Message.CreateInstance(msgName);
            if (msgClass != null)
            {
                //记录允许的消息
                AllowedMsgList[client].Enqueue(msgClass);
                //允许发送
                this.SendCommand(client, CommandSymbol.All, Encoding.Unicode.GetBytes(msgClass.MsgName));
            }
        }

        /// <summary>
        /// 处理允许发送命令
        /// </summary>
        private void CommandAllow(Command cmd, SecurityTCP.Client client)
        {
            //获得消息名
            string msgName = Encoding.Unicode.GetString(cmd.Data, 0, cmd.Data.Length);
            //通知已经允许发送
            AllowEventHandler?.Invoke(this, new Tuple<SecurityTCP.Client, string>(client, msgName));
        }

        /// <summary>
        /// 处理实体命令
        /// </summary>
        private void CommandEntity(Command cmd, SecurityTCP.Client client)
        {
            var queue = AllowedMsgList[client];
            while (queue.Count > 0)
            {
                var msgClass = queue.Dequeue();
                //反序列化成Message对象
                var realMsg = Message.DeserializeMessage(msgClass.MsgName, cmd.Data, 0, cmd.Data.Length);
                //判断是否是允许的消息
                if (realMsg != null)
                {
                    //指定Endpoint
                    realMsg.Endpoint = EndpointList[client];
                    //向上层推送消息
                    this.PushMessge(realMsg);
                    break;
                }
            }
        }

        /// <summary>
        /// 处理刷新命令
        /// </summary>
        private void CommandFlush(Command cmd, SecurityTCP.Client client)
        {
            //清理允许的消息
            var queue = AllowedMsgList[client];
            queue.Clear();
        }

        #endregion

    }

}
