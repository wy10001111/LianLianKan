using LianLianKan.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private MyUser _user;
        public MyUser User
        {
            get
            {
                return _user;
            }
            set
            {
                if (Set(ref _user, value))
                {
                    this.LoadUserHead();
                    _user.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(_user.HeadStream))
                            this.LoadUserHead();
                    };
                }
            }
        }

        protected virtual bool Set<T>(ref T item, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.ReferenceEquals(item, value))
            {
                item = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
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
