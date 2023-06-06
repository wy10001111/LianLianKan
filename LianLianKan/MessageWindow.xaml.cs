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
using System.Windows.Shapes;

namespace LianLianKan
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
        }
        public string Caption { get; } = "Hi !";
        public string MessageBoxText { get; set; }

        public MessageWindow(Window owner, string messageBoxText)
        {
            this.MessageBoxText = messageBoxText;
            Owner = owner;
            InitializeComponent();
            this.DataContext = this;
        }

        public MessageWindow(Window owner, string messageBoxText, string caption) : this(owner, messageBoxText)
        {
            this.Caption = caption;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static void Show(Window owner, string messageBoxText)
        {
            new MessageWindow(owner, messageBoxText).ShowDialog();
        }

        public static void Show(Window owner, string messageBoxText, string caption)
        {
            new MessageWindow(owner, messageBoxText, caption).ShowDialog();
        }

    }
}
