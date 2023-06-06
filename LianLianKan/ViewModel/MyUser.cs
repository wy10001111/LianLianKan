using LianLianKanLib;
using LianLianKanLib.Protocol;
using LianLianKanLib.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LianLianKan.ViewModel
{
    public class MyUser :User
    {
        public MyUser(int id, string account, string password, string name)
            : base(id, account, password, name)
        {
            _clientProtocol = new ClientProtocol();
            _clientProtocol.ErrorEventHandler += (sender, error)
                => ErrorEventHandler?.Invoke(error.Content);
            _clientProtocol.OfflineEventHandler += (endpoint)
                => OfflineEventHandler?.Invoke();
        }

        ~MyUser()
        {
            _clientProtocol.Disconnect();
            _clientProtocol.Dispose();
        }

        #region 变量与属性

        /// <summary>
        /// 通讯协议
        /// </summary>
        private ClientProtocol _clientProtocol;

        /// <summary>
        /// 离线事件
        /// </summary>
        public event Action OfflineEventHandler;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event Action<string> ErrorEventHandler;

        #endregion

        #region 方法

        private bool SendMessage(Message msg, Action<Message> feedback)
        {
            using (var signal = new AutoResetEvent(false))
            {
                EventHandler<Message> waitBack = (object sd, Message respondsMsg) =>
                {
                    if (msg.CallID != respondsMsg.CallID)
                        return;
                    feedback.Invoke(respondsMsg);
                    signal.Set();
                };

                _clientProtocol.MessageEventHandler += waitBack;
                _clientProtocol.SendMsg(msg);
                if (signal.WaitOne(2000) == false)
                {
                    _clientProtocol.MessageEventHandler -= waitBack;
                    return false;
                }
                _clientProtocol.MessageEventHandler -= waitBack;
                return true;
            }
        }

        private bool SendBigMessage(Message msg, Action<Message> feedback)
        {
            using (var signal = new AutoResetEvent(false))
            {
                EventHandler<Message> waitBack = (object sd, Message respondsMsg) =>
                {
                    if (msg.CallID != respondsMsg.CallID)
                        return;
                    feedback.Invoke(respondsMsg);
                    signal.Set();
                };

                _clientProtocol.MessageEventHandler += waitBack;
                _clientProtocol.SendBigMsg(msg);
                if (signal.WaitOne(2000) == false)
                {
                    _clientProtocol.MessageEventHandler -= waitBack;
                    return false;
                }
                _clientProtocol.MessageEventHandler -= waitBack;
                return true;
            }
        }

        /// <summary>
        /// 注册
        /// </summary>
        public bool Register()
        {
            string error = null;
            //连接服务器
            if (_clientProtocol.Connect("127.0.0.2", 69))
            {
                //创建消息
                var msg = new MessageRegisterRequest()
                {
                    CallID = DateTime.Now.GetHashCode(),
                    UserID = Account.GetHashCode(),
                    Account = this.Account,
                    Password = this.Password,
                    Name = this.Name,
                    Introduce = this.Introduce,
                    Endpoint = _clientProtocol.Endpoint,
                    HeadsculptStream = this.HeadStream.GetBuffer()
                };
                //发送消息
                var result = this.SendBigMessage(msg, (respondsMsg) => {
                    //服务器回复后处理
                    if (respondsMsg.MessageID == MessageID.MI_REGISTER_FAILED)
                    {
                        //注册失败消息
                        error = (respondsMsg as MessageRegisterFailed).Error;
                    }
                    else if (respondsMsg.MessageID == MessageID.MI_REGISTER_SUCCESSFULLY)
                    {
                        //注册成功消息
                        var realMsg = (respondsMsg as MessageRegisterSuccessfully);
                        error = null;
                    }
                    else
                        error = "服务器响应错误！";
                });
                if (result == false)
                    error = "登录超时，请重试！";
            }
            else
                error = "无法连接服务器！";
            _clientProtocol.Disconnect();
            if (error == null)
                return true;
            ErrorEventHandler.Invoke(error);
            return false;
        }

        /// <summary>
        /// 登录
        /// </summary>
        public bool Login()
        {
            string error = null;
            //连接服务器
            if (_clientProtocol.Connect("127.0.0.2", 69))
            {
                //创建消息
                var msg = new MessageLoginRequest()
                {
                    CallID = DateTime.Now.GetHashCode(),
                    UserID = Account.GetHashCode(),
                    Endpoint = this._clientProtocol.Endpoint,
                    Account = Account,
                    Password = Password
                };
                //发送消息
                var result = this.SendMessage(msg, (respondsMsg) => {
                    //服务器返回后处理
                    if (respondsMsg.MessageID == MessageID.MI_LOGIN_FAILED)
                        //登录失败
                        error = (respondsMsg as MessageLoginFailed).Error;
                    else if (respondsMsg.MessageID == MessageID.MI_LOGIN_RESPONSE)
                    {//登录成功
                        var realMsg = respondsMsg as MessageLoginResponds;
                        this.ID = realMsg.UserID;
                        this.Account = realMsg.Account;
                        this.Name = realMsg.Name;
                        this.Introduce = realMsg.Introduce;
                        this.Endpoint = realMsg.Endpoint;
                        error = null;
                    }
                    else
                        error = "服务器响应错误！";
                });
                if (result == false)
                    error = "登录超时，请重试！";
            }
            else
                error = "无法连接服务器！";
            if (error == null)
            {
                this.GetHeadsculpt();
                return true;
            }
            ErrorEventHandler.Invoke(error);
            return false;
        }

        /// <summary>
        /// 退出
        /// </summary>
        public bool Logout()
        {
            return _clientProtocol.Disconnect();
        }

        /// <summary>
        /// 添加一个游戏记录
        /// </summary>
        public new void AddGameRecord(TimeSpan spentTime)
        {
            //添加到列表
            base.AddGameRecord(spentTime);
            if (_clientProtocol.IsConnected)
            {
                //发送给服务器
                var record = this.LastGameRecord();
                var msg = new MessageTakeOneGameRecord()
                {
                    CallID = DateTime.Now.GetHashCode(),
                    UserID = this.ID,
                    Record = new LianLianKanLib.Protocol.Messages.GameRecord() { PlayTime = record.PlayTime, SpentTime = record.SpentTime },
                    Endpoint = this.Endpoint
                };
                this._clientProtocol.SendMsg(msg);
            }
        }

        /// <summary>
        /// 获得头像
        /// </summary>
        private bool GetHeadsculpt()
        {
            string error = null;
            var request = new MessageGetHeadRequest()
            {
                CallID = DateTime.Now.GetHashCode(),
                UserID = this.ID,
                Endpoint = this.Endpoint,
            };
            MessageGetHeadResponds responds = null;
            var result = this.SendMessage(request, (respondsMsg) => {
                if (respondsMsg.MessageID == MessageID.MI_ERROR)
                {
                    var realMsg = respondsMsg as MessageError;
                    error = realMsg.Error;
                }
                else if (respondsMsg.MessageID == MessageID.MI_GET_HEAD_RESPONSE)
                {
                    responds = respondsMsg as MessageGetHeadResponds;
                    error = null;
                }
                else
                    error = "服务器响应错误！";
            });
            if (result == false)
                error = "服务器响应失败！";

            if (error == null)
            {
                App.Current.Dispatcher.Invoke(() => {
                    if (responds?.Source != null)
                        this.ChangeHeadStream(new MemoryStream(responds.Source));
                });
                return true;
            }
            ErrorEventHandler.Invoke(error);
            return false;
        }

        /// <summary>
        /// 获得游戏记录
        /// </summary>
        public bool GetGameRecords()
        {
            string error = null;
            var request = new MessageGetGameRecords()
            {
                CallID = DateTime.Now.GetHashCode(),
                UserID = this.ID,
                Endpoint = this.Endpoint,
            };
            var result = this.SendMessage(request, (respondsMsg) => {
                if (respondsMsg.MessageID == MessageID.MI_ERROR)
                {
                    var realMsg = respondsMsg as MessageError;
                    error = realMsg.Error;
                }
                else if (respondsMsg.MessageID == MessageID.MI_GAME_RECORDS)
                {
                    var gameRecordMsg = respondsMsg as MessageGameRecords;
                    this.GameRecords.Clear();
                    if (gameRecordMsg.Records != null)
                    {
                        foreach (var record in gameRecordMsg.Records)
                            this.GameRecords.Add(new LianLianKanLib.GameRecord(record.PlayTime, record.SpentTime));
                    }
                }
                else
                    error = "服务器响应错误！";
            });
            if (result == false)
                error = "服务器响应失败！";

            if (error == null)
                return true;
            ErrorEventHandler?.Invoke(error);
            return false;
        }


        #endregion

    }
}
