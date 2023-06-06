using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LianLianKan
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var loginWin = new LoginWindow();
                loginWin.ShowDialog();
                switch (loginWin.LoginResult)
                {
                    case LoginResult.LoginResultTourist:
                    case LoginResult.LoginResultLoginSuccesful:
                        {
                            var user = loginWin.User;
                            new MainWindow(user).ShowDialog();
                            user.Logout();
                            break;
                        }
                    case LoginResult.LoginResultWorking:
                    case LoginResult.LoginResultLoginFail:
                    case LoginResult.LoginResultRegist:
                    case LoginResult.LoginResultExit:
                    default:
                        break;
                }
            }
            catch (Exception)
            {

            }
            this.Shutdown();
            base.OnStartup(e);
        }

    }
}
