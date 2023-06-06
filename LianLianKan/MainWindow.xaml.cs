using LianLianKan.ViewModel;
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

namespace LianLianKan
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MyUser user)
        {
            InitializeComponent();
            _myUser = user;
        }

        private MyUser _myUser;

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            string str = playButton.Content as string;
            if (str == "开始游戏")
            {
                playButton.Content = "重新开始";
                this.map.StartGame();
            }
            else
            {
                gameAlarm.StopTiming();
                this.map.ResetGame();
            }
            gameAlarm.StartTiming();
        }

        private void OnGameOver(object sender, RoutedEventArgs e)
        {
            gameAlarm.StopTiming();
            this._myUser.AddGameRecord(gameAlarm.GameTime);
            var window = new CongratulationWindow(gameAlarm.GameTime) { Owner = this };
            window.ShowDialog();
            playButton.Content = "开始游戏";
        }

        private void OnStopGame(object sender = null, RoutedEventArgs e = null)
        {
            this.map.StopGame();
            this.gameAlarm.StopTiming();
            playButton.Content = "开始游戏";
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.OnStopGame();
            this._myUser?.Logout();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            this.OnStopGame();
            this._myUser.Logout();
            this.Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            userInfo.User = _myUser;
            _myUser.GetGameRecords();
        }
    }
}
