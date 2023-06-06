using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;

namespace LianLianKan.Control
{
    class LianLianKanButton : ToggleButton
    {
        public int PairID { get; set; } //连连看按钮配对ID
        public int Row { get; set; }  //行
        public int Col { get; set; }  //列
        public LianLianKanButton(int pariID, int row, int col, Brush brush) : base()
        {
            this.PairID = pariID;
            this.Row = row;
            this.Col = col;
            this.Background = brush;
            this.Style = App.Current.FindResource("LianLianKanButtonStyle") as System.Windows.Style;
            this.Content = $"{pariID}";
        }
    }

}
