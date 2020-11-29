using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LianLianKan
{
    public class MyClientProtocol : ClientProtocol
    {
        public void Connect()
        {
            this.Connect("127.0.0.2", 69);
        }
    }

    class UserManager
    {
        private MyClientProtocol _clientProtocol;
        public Window Owner { get; set; }
        private UIUser _currentUser;
        public UIUser CurrentUser
        {
            get
            {
                return _currentUser;
            }
            private set
            {
                if (_currentUser != value)
                {
                    if (null != _currentUser)
                    {
                        _currentUser.HeadStreamPropertyChanged -= UserHeadChanged;
                        _currentUser.PropertyChanged -= UserPropertyChanged;
                    }
                    _currentUser = value;
                    if (null != _currentUser)
                    {
                        _currentUser.HeadStreamPropertyChanged += UserHeadChanged;
                        _currentUser.PropertyChanged += UserPropertyChanged;
                    }
                }
            }
        }
        public UserManager(Window owner)
        {
            this.Owner = owner;
            _clientProtocol = new MyClientProtocol();
            _clientProtocol.ErrorEvent += ErrorResponds;
            _clientProtocol.OfflineEvent += OfflineResponds;
            _clientProtocol.MessageEvent += MessegeResponds;
        }
        ~UserManager()
        {
            _clientProtocol.MessageEvent -= MessegeResponds;
            _clientProtocol.OfflineEvent -= OfflineResponds;
            _clientProtocol.ErrorEvent -= ErrorResponds;
            _clientProtocol.Disconnect();
            _clientProtocol.Dispose();
            CurrentUser = null;
        }
        public void ErrorResponds(object sender, ProtocolError e)
        {
            App.Current.Dispatcher.Invoke(() => {
                MessageWindow.Show(Owner, e.Error);
            });
        }

        public void OfflineResponds()
        {
            App.Current.Dispatcher.Invoke(() => {
                //掉线只能提示一下，绝不允许界面上改变任何事物。
                MessageWindow.Show(this.Owner, "服务器已经失联 ！");
            });
        }
        public void MessegeResponds(object sender, Message e)
        {
        }

        public UIUser Login()
        {
            CurrentUser = null;
            var window = new LoginWindow(Owner, _clientProtocol);
            window.ShowDialog();
            switch (window.LoginResult)
            {
                case LoginResult.LoginResultTourist:
                    {
                        CurrentUser = window.User;
                        break;
                    }
                case LoginResult.LoginResultLoginSuccesful:
                    {
                        CurrentUser = window.User;
                        this.GetGameRecords();
                        Task.Run(() => {
                            this.GetHeadsculpt();
                        });
                        break;
                    }
                case LoginResult.LoginResultWorking:
                case LoginResult.LoginResultLoginFail:
                case LoginResult.LoginResultRegist:
                case LoginResult.LoginResultExit:
                default:
                    CurrentUser = null;
                    break;
            }
            return CurrentUser;
        }

        public void Logout()
        {
            _clientProtocol.Disconnect();
            CurrentUser = null;
        }

        public void TakeDownGameRecord(TimeSpan record)
        {
            CurrentUser.AddGameRecord(record);
            if (_clientProtocol.IsConnected)
            {
                var msg = new MessageTakeOneGameRecord()
                {
                    CallID = DateTime.Now.GetHashCode(),
                    UserID = CurrentUser.ID,
                    Record = CurrentUser.LastGameRecord(),
                    User = CurrentUser
                };
                this._clientProtocol.SendMsg(msg);
            }
        }
        
        public void UserHeadChanged(Object sender, PropertyChangedEventArgs e)
        {

        }

        public void UserPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {

        }

        private bool GetHeadsculpt()
        {
            if (CurrentUser == null)
                return false;
            string error = "";
            var request = new MessageGetHeadRequest()
            {
                CallID = DateTime.Now.GetHashCode(),
                UserID = CurrentUser.ID,
                User = CurrentUser,
            };
            MessageGetHeadResponds responds = null;
            using (var signal = new ManualResetEventSlim(false))
            {
                EventHandler<Message> waitBack = (object sd, Message respondsMsg) =>
                {
                    if (request.CallID != respondsMsg.CallID || request.UserID != respondsMsg.UserID)
                        return;
                    if (respondsMsg.MessageID == MessageID.MI_ERROR)
                    {
                        var realMsg = respondsMsg as MessageError;
                        error = realMsg.Error;
                        signal.Set();
                    }
                    else if (respondsMsg.MessageID == MessageID.MI_GET_HEAD_RESPONSE)
                    {
                        responds = respondsMsg as MessageGetHeadResponds;
                        signal.Set();
                    }
                };
                _clientProtocol.MessageEvent += waitBack;
                this._clientProtocol.SendMsg(request);
                if (signal.Wait(6000) == false)
                    error = "服务器响应超时。";
                _clientProtocol.MessageEvent -= waitBack;
            }
            if (responds != null)
                App.Current.Dispatcher.Invoke(() => {
                    CurrentUser.ChangeHeadStream(new MemoryStream(responds.Source));
                });
                //MessageWindow.Show(this.Owner, "无法获得头像！原因：" + error);
            return responds != null;
        }

        private bool GetGameRecords()
        {
            if (CurrentUser == null)
                return false;
            string error = "";
            var request = new MessageGetGameRecords()
            {
                CallID = DateTime.Now.GetHashCode(),
                UserID = CurrentUser.ID,
                User = CurrentUser,
            };
            MessageGameRecords gameRecordMsg = null;
            using (var signal = new ManualResetEventSlim(false))
            {
                EventHandler<Message> waitBack = (object sd, Message respondsMsg) =>
                {
                    if (request.CallID != respondsMsg.CallID || respondsMsg.UserID != request.UserID)
                        return;
                    if (respondsMsg.MessageID == MessageID.MI_ERROR)
                    {
                        var realMsg = respondsMsg as MessageError;
                        error = realMsg.Error;
                        signal.Set();
                    }
                    else if (respondsMsg.MessageID == MessageID.MI_GAME_RECORDS)
                    {
                        gameRecordMsg = respondsMsg as MessageGameRecords;
                        signal.Set();
                    }
                };
                _clientProtocol.MessageEvent += waitBack;
                this._clientProtocol.SendMsg(request);
                if (signal.Wait(1500) == false)
                    error = "服务器响应超时。";
                _clientProtocol.MessageEvent -= waitBack;
            }
            if (gameRecordMsg != null)
            {
                CurrentUser.GameRecords.Clear();
                foreach (var record in gameRecordMsg.Records)
                    CurrentUser.GameRecords.Add(record);
                return true;
            }
            MessageWindow.Show(this.Owner, "无法获得游戏记录！原因：" + error);
            return false;
        }


    }
}
