using LianLianKan.Control;
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
            _userManager = new UserManager(this);
        }

        private UserManager _userManager;

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
            this._userManager.TakeDownGameRecord(gameAlarm.GameTime);
            var window = new WindowCongratulations(gameAlarm.GameTime) { Owner = this };
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
            this._userManager.Logout();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            this.OnStopGame();
            this._userManager.Logout();
            userInfo.User = null;
            userInfo.User = _userManager.Login();
            if (userInfo.User == null)
            {
                this.Close();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.OnLogout(sender, e);
        }
    }
}
