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
using System.Windows.Shapes;

namespace LianLianKan
{
    /// <summary>
    /// CongratulationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CongratulationWindow : Window
    {
        public CongratulationWindow(TimeSpan timeSpent)
        {
            InitializeComponent();
            time.Content = timeSpent.ToString(GameRecord.SpentTimeFormat);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
