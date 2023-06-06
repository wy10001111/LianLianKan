using LianLianKanLib;
using LianLianKanLib.Protocol;
using LianLianKanLib.Protocol.Messages;
using LianLianKanServer.MessageTriggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LianLianKanServer
{
    public class MainManager
    {

        public SqlServer _sqlServer;
        public ServerProtocol _serverProtocol;
        public ObservableCollection<User> UserList { get; } 
            = new ObservableCollection<User>();

        #region 方法

        public User FindUser(int id)
        {
            User realUser = null;
            for (int index = 0; index < this.UserList.Count; index++)
            {
                var user = this.UserList[index];
                if (user.ID == id)
                {
                    realUser = user;
                    break;
                }
            }
            return realUser;
        }

        public User FindUser(string account)
        {
            for (int index = 0; index < this.UserList.Count; index++)
            {
                var user = this.UserList[index];
                if (user.Account == account)
                    return user;
            }
            return null;
        }

        public User FindUser(Endpoint endpoint)
        {
            for (int index = 0; index < this.UserList.Count; index++)
            {
                var user = this.UserList[index];
                if (user.Endpoint == endpoint)
                    return user;
            }
            return null;
        }

        private void OnErrorEvent(object s, ProtocolError error)
        {
            App.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(error.Content);
            });
        }

        private void OnOfflineEvent(Endpoint endpoint)
        {
            var user = FindUser(endpoint);
            if (user != null)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    user.Endpoint = null;
                });
            }
        }

        public bool OpenServer(string sqlAccount, string sqlPassword, string ip, int port)
        {
            _sqlServer = new SqlServer();
            //开启数据库
            if (_sqlServer.OpenDatabase(sqlAccount, sqlPassword))
            {
                _serverProtocol = new ServerProtocol();
                //监听事件
                _serverProtocol.ErrorEventHandler += OnErrorEvent;
                _serverProtocol.OfflineEventHandler += OnOfflineEvent;
                //初始化消息触发器MessageTrigger
                MessageTrigger.Manager = this;
                _serverProtocol.MessageEventHandler += MessageTrigger.ReceiveMessage;
                //启动服务
                if (_serverProtocol.StartServing(ip, port))
                {
                    //打开成功
                    _sqlServer.InitUserList(this.UserList);
                    return true;
                }
            }
            //打开失败
            this.CloseServer();
            return false;
        }

        public void CloseServer()
        {
            //关闭数据库
            _sqlServer?.CloseDatabase();
            _sqlServer = null;
            //关闭网络
            _serverProtocol?.StopServing();
            _serverProtocol?.Dispose();
            _serverProtocol = null;
            //清理列表
            UserList.Clear();
        }

        #endregion
    }
}
