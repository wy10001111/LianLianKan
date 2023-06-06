using LianLianKanLib;
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
            this.DataContext = this.Manager;
            usernameBox.Text = "root";
            passwordBox.Password = "123456";
        }

        public MainManager Manager { get; } = new MainManager();

        #region 方法

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.OnCloseServer(this, new RoutedEventArgs());
        }

        private void OnOpenServer(object sender, RoutedEventArgs e)
        {
            if (Manager.OpenServer(usernameBox.Text, passwordBox.Password, hostBox.Text, int.Parse(portBox.Text)))
            {
                //打开成功
                VisualStateManager.GoToElementState(this, "ServingState", true);
            }
        }

        private void OnCloseServer(object sender, RoutedEventArgs e)
        {
            Manager.CloseServer();
            VisualStateManager.GoToElementState(this, "ClosingState", true);
        }

        private void OnlineFilter(object sender, FilterEventArgs e)
        {
            User user = e.Item as User;
            e.Accepted = user.Endpoint != null;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userDataGrid != e.OriginalSource)
                return;
            DataGrid dataGrid = sender as DataGrid;
            User user = dataGrid.SelectedItem as User;
            if (null != user)
                Manager._sqlServer.GetGameRecords(user);
        }

        #endregion

    }
}
