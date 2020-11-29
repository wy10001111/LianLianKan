using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace LianLianKan.Control
{
    /// <summary>
    /// UserInfomationControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserInfomationControl : UserControl, INotifyPropertyChanged
    {
        public UserInfomationControl()
        {
            InitializeComponent();
        }

        private UIUser _user;
        public UIUser User
        {
            get
            {
                return _user;
            }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    if (null != value)
                    {
                        this.LoadUserHead();
                        _user.HeadStreamPropertyChanged += UserHeadChanged;
                    }
                    else
                        userHead.Source = null;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(User)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UserHeadChanged(object sender, PropertyChangedEventArgs e)
        {
            this.LoadUserHead();
        }

        public void LoadUserHead()
        {
            if (User.HeadStream != null)
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = User.HeadStream;
                image.EndInit();
                userHead.Source = image;
            }
        }
    }
}
