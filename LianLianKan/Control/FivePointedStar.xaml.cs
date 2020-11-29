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

namespace LianLianKan.Control
{
    /// <summary>
    /// FivePointedStar.xaml 的交互逻辑
    /// </summary>
    public partial class FivePointedStar : UserControl
    {
        public FivePointedStar()
        {
            InitializeComponent();
            var a = FrameworkElement.MouseMoveEvent;
        }

        public static readonly DependencyProperty FillColorProperty
            = DependencyProperty.Register(nameof(FillColor), typeof(Color), typeof(FivePointedStar),
                new PropertyMetadata(Colors.CadetBlue, FillColorChanged));
        public Color FillColor { get; set; }
        private static void FillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var star = d as FivePointedStar;
            star.fillColor.Color = (Color)(e.NewValue);
        }

        public static readonly DependencyProperty StrokeColorProperty
            = DependencyProperty.Register(nameof(StrokeColor), typeof(Color), typeof(FivePointedStar),
                new PropertyMetadata(Colors.AliceBlue, StrokeColorChanged));
        public Color StrokeColor { get; set; }
        private static void StrokeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var star = d as FivePointedStar;
            star.strokeColor.Color = (Color)(e.NewValue);
        }

        public static readonly DependencyProperty ScaleValueProperty
            = DependencyProperty.Register(nameof(ScaleValue), typeof(double), typeof(FivePointedStar),
                new PropertyMetadata(1.0, ScaleValueChanged));
        public double ScaleValue { get; set; }
        private static void ScaleValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var star = d as FivePointedStar;
            star.starScale.ScaleX = (double)(e.NewValue);
            star.starScale.ScaleY = (double)(e.NewValue);
        }

        public static readonly DependencyProperty TranslateXProperty
            = DependencyProperty.Register(nameof(TranslateX), typeof(double), typeof(FivePointedStar),
                new PropertyMetadata(0.0, TranslateXChanged));
        public double TranslateX { get; set; }
        private static void TranslateXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var star = d as FivePointedStar;
            star.starTranslate.X = (double)(e.NewValue);
        }

        public static readonly DependencyProperty TranslateYProperty
            = DependencyProperty.Register(nameof(TranslateY), typeof(double), typeof(FivePointedStar),
                new PropertyMetadata(0.0, TranslateYChanged));
        public double TranslateY { get; set; }
        private static void TranslateYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var star = d as FivePointedStar;
            star.starTranslate.Y = (double)(e.NewValue);
        }

        public static readonly DependencyProperty RotateAngleProperty
            = DependencyProperty.Register(nameof(RotateAngle), typeof(double), typeof(FivePointedStar),
                new PropertyMetadata(0.0, RotateAngleChanged));
        public double RotateAngle { get; set; }
        private static void RotateAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var star = d as FivePointedStar;
            star.starRotate.Angle = (double)(e.NewValue);
        }
    }
}
