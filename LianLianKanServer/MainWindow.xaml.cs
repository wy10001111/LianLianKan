using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using LianLianKanLib;
using System.Reflection;
using System.Collections.Specialized;

namespace LianLianKanServer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this._mainConfig;
            usernameBox.Text = "root";
            passwordBox.Password = "qq666999MS";
        }

        MainConfig _mainConfig = new MainConfig();

        private void OnClosing(object sender, CancelEventArgs e)
        {
            this.OnCloseServer(this, new RoutedEventArgs());
        }

        private void OnOpenServer(object sender, RoutedEventArgs e)
        {
            _mainConfig._sqlServer = new SqlServer();
            if (_mainConfig._sqlServer.OpenDatabase(usernameBox.Text, passwordBox.Password))
            {
                _mainConfig._serverProtocol = new ServerProtocol();
                _mainConfig._serverProtocol.ErrorEvent += (object s, ProtocolError error) =>
                {
                    App.Current.Dispatcher.Invoke(() => {
                        MessageBox.Show(error.Error);
                    });
                };
                Assembly _msgAssembly = Assembly.Load("LianLianKanServer");
                _mainConfig._serverProtocol.MessageEvent += (object o, Message msg) =>
                {
                    App.Current.Dispatcher.Invoke(() => {
                        var action = _msgAssembly.CreateInstance("LianLianKanServer.ActionFor" + msg.MsgName) as MessageAction;
                        action?.StartWork(_mainConfig, msg);
                    });
                };
                if (_mainConfig._serverProtocol.StartServing(hostBox.Text, int.Parse(portBox.Text)))
                {
                    //打开成功
                    VisualStateManager.GoToElementState(this, "ServingState", true);
                    _mainConfig._sqlServer.InitUserList(_mainConfig.UserList);
                    return;
                }
            }
            //打开失败
            this.OnCloseServer(this, new RoutedEventArgs());
        }

        private void OnCloseServer(object sender, RoutedEventArgs e)
        {
            _mainConfig._sqlServer?.CloseDatabase();
            _mainConfig._sqlServer = null;
            _mainConfig._serverProtocol?.StopServing();
            _mainConfig._serverProtocol?.Dispose();
            _mainConfig._serverProtocol = null;
            _mainConfig.UserList.Clear();
            VisualStateManager.GoToElementState(this, "ClosingState", true);
        }

        private void OnlineFilter(object sender, FilterEventArgs e)
        {
            UIUser user = e.Item as UIUser;
            e.Accepted = user.Client != null;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userDataGrid != e.OriginalSource)
                return;
            DataGrid dataGrid = sender as DataGrid;
            UIUser user = dataGrid.SelectedItem as UIUser;
            if (null != user)
                _mainConfig._sqlServer.GetGameRecords(user);
        }
        
    }


}
