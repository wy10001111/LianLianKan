using LianLianKanLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    /// GameAlarm.xaml 的交互逻辑
    /// </summary>
    public partial class GameAlarm : UserControl, INotifyPropertyChanged, IDisposable
    {
        public GameAlarm()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            _isTiming = false;
            _canTiming = false;
        }
        public void Dispose()
        {
            this.SizeChanged -= OnSizeChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isTiming;
        private bool _canTiming;
        private TimeSpan _gameTime;
        public TimeSpan GameTime
        {
            get
            {
                return _gameTime;
            }
            set
            {
                if (_gameTime != value)
                {
                    _gameTime = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(GameTime)));
                }
            }
        }

        public void UpdateGameTime(object sender, EventArgs e)
        {
            GameTime += TimeSpan.FromMilliseconds(10);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewBox.Width = e.NewSize.Width;
            viewBox.Height = e.NewSize.Height * (78.0 / 105.0);
        }

        public void StartTiming()
        {
            _canTiming = true;
            _isTiming = true;
            Task.Run(async () => {
                GameTime = new TimeSpan(0);
                var startTime = DateTime.Now;
                while (_canTiming)
                {
                    await Task.Delay(1);
                    var a = DateTime.Now - startTime;
                    GameTime = (DateTime.Now - startTime);
                }
                string text = GameTime.ToString(GameRecord.SpentTimeFormat);
                _isTiming = false;
            });
        }

        public void StopTiming()
        {
            _canTiming = false;
            while (_isTiming)
            {
                Task.Delay(1).Wait();
            }
        }

    }

    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class GameTimeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan time = (TimeSpan)value;
            return time.ToString(GameRecord.SpentTimeFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.Parse(value as string);
        }

    }

}
