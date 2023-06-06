using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LianLianKan.Control
{
    static class LianLianKanBrush
    {
        public static int NumberOfBrush { get; } = 30;
        private static SolidColorBrush[] _solidColorBrushes;
        public static Brush GetSolidColorBrush(int id)
        {
            if (_solidColorBrushes == null)
            {
                _solidColorBrushes = new SolidColorBrush[NumberOfBrush];
                int count = 0;
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Teal);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Lime);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Crimson);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Cyan);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Navy);

                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Gold);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Purple);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.DarkGoldenrod);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Chocolate);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Maroon);

                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Indigo);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Green);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Yellow);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.LightSeaGreen);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Aquamarine);

                _solidColorBrushes[count++] = new SolidColorBrush(Colors.OliveDrab);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.RoyalBlue);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Orange);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Gray);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Pink);

                _solidColorBrushes[count++] = new SolidColorBrush(Colors.BurlyWood);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.CadetBlue);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.CornflowerBlue);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Magenta);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.HotPink);

                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Black);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.SpringGreen);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.Silver);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.RosyBrown);
                _solidColorBrushes[count++] = new SolidColorBrush(Colors.YellowGreen);
            }
            return _solidColorBrushes[id];
        }
    }

}
