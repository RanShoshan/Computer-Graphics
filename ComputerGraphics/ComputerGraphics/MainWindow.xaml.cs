/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComputerGraphics {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
   

    public enum UserState
    {
        NONE = 0,
        BTN_LINE_1ST_CLICK = 1,
        BTN_LINE_2ST_CLICK = 2,
        BTN_CIRCLE_1ST_CLICK ,
        BTN_CIRCLE_2ST_CLICK ,
        BTN_BEZIER_1ST_CLICK,
        BTN_BEZIER_2ND_CLICK,
        BTN_BEZIER_3RD_CLICK,
        BTN_BEZIER_4TH_CLICK
    }

    public enum PixelStyle
    {
        DEFAULT = 0,
        BOLD
    }

    

    public class Bezier {
        public Point cp1, cp2, cp3, cp4;
    }

    public partial class MainWindow : Window {

        public UserState state = UserState.NONE;
        public Point lastPoint = new Point();
        public Bezier bezier = new Bezier();

        public const int STROKE_BOLD = 10;


        public MainWindow()
        {
            InitializeComponent();
            tbBezierNumOfLines.IsEnabled = false;
            //implement this //ToggleOffAllButtons();
        }
        



        public void OnBtnCircleClicked(object sender, RoutedEventArgs e)
        {

            state = UserState.BTN_CIRCLE_1ST_CLICK;
        }


        public void DrawCircle(Point p1, Point p2)
        {
            int xCenter = Convert.ToInt32(p2.X);
            int yCenter = Convert.ToInt32(p2.Y);
            int xr = Convert.ToInt32(p1.X);
            int yr = Convert.ToInt32(p1.Y);

            var a = (xCenter - xr);
            var b = (yCenter - yr);
            var r = Math.Sqrt(a * a + b * b);

            for (double i = 0.0; i < 360.0; i += 0.1)
            {
            double angle = i * System.Math.PI / 180;
            int x = (int)(xr + r * System.Math.Cos(angle));
            int y = (int)(yr + r * System.Math.Sin(angle));
                    SetPixel(x, y);
            }

        }
        

        public void OnBtnLineClicked(object sender, RoutedEventArgs e) {

            state = UserState.BTN_LINE_1ST_CLICK;
        }

        public void OnBtnBezierClicked(object sender, RoutedEventArgs e) {

            state = UserState.BTN_BEZIER_1ST_CLICK;
            tbBezierNumOfLines.IsEnabled = true;
        }

        private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        public void Line(Point p1, Point p2)
        {
            int x0 = Convert.ToInt32(p1.X);
            int y0 = Convert.ToInt32(p1.Y);
            int x1 = Convert.ToInt32(p2.X);
            int y1 = Convert.ToInt32(p2.Y);
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
            if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
            int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (!(steep ? SetPixel(y, x) : SetPixel(x, y))) return;
                err = err - dY;
                if (err < 0) { y += ystep; err += dX; }
            }
        }

        public void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (state)
            {
                case UserState.NONE:
                    break;
                case UserState.BTN_LINE_1ST_CLICK:
                    lastPoint = e.GetPosition(myCanvas);
                    state = UserState.BTN_LINE_2ST_CLICK;
                    break;
                case UserState.BTN_LINE_2ST_CLICK:
                    Line(lastPoint, e.GetPosition(myCanvas));
                    state = UserState.BTN_LINE_1ST_CLICK;
                    break;

                case UserState.BTN_CIRCLE_1ST_CLICK:
                    lastPoint = e.GetPosition(myCanvas);
                    state = UserState.BTN_CIRCLE_2ST_CLICK;
                    break;
                case UserState.BTN_CIRCLE_2ST_CLICK:
                    DrawCircle(lastPoint, e.GetPosition(myCanvas));
                    state = UserState.BTN_CIRCLE_1ST_CLICK;
                    break;

                case UserState.BTN_BEZIER_1ST_CLICK:
                    state = UserState.BTN_BEZIER_2ND_CLICK;
                    bezier.cp1 = e.GetPosition(myCanvas);
                    SetPixel(Convert.ToInt32(bezier.cp1.X), Convert.ToInt32(bezier.cp1.Y), PixelStyle.BOLD);
                    break;
                case UserState.BTN_BEZIER_2ND_CLICK:
                    state = UserState.BTN_BEZIER_3RD_CLICK;
                    bezier.cp2 = e.GetPosition(myCanvas);
                    SetPixel(Convert.ToInt32(bezier.cp2.X), Convert.ToInt32(bezier.cp2.Y), PixelStyle.BOLD);
                    break;
                case UserState.BTN_BEZIER_3RD_CLICK:
                    state = UserState.BTN_BEZIER_4TH_CLICK;
                    bezier.cp3 = e.GetPosition(myCanvas);
                    SetPixel(Convert.ToInt32(bezier.cp3.X), Convert.ToInt32(bezier.cp3.Y), PixelStyle.BOLD);
                    break;
                case UserState.BTN_BEZIER_4TH_CLICK:
                    bezier.cp4 = e.GetPosition(myCanvas);
                    SetPixel(Convert.ToInt32(bezier.cp4.X), Convert.ToInt32(bezier.cp4.Y), PixelStyle.BOLD);
                    DrawBezierCurve(bezier);

                    break;
                default:
                    break;
            }

        }

        public void DrawBezierCurve(Bezier b) {

        }

        private bool SetPixel(int x, int y, PixelStyle style = PixelStyle.DEFAULT) {

            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = System.Windows.Media.Brushes.Blue;
            if(style == PixelStyle.BOLD) {
                rect.StrokeThickness = STROKE_BOLD;
                rect.Width = STROKE_BOLD;
                rect.Height = STROKE_BOLD;
            }
            else {
                rect.StrokeThickness = 1;
                rect.Width = 1;
                rect.Height = 1;
            }
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            myCanvas.Children.Add(rect);
            return true;

        }

        public void ToggleOffAllButtons() {
            foreach (var item in mainToolbar.Items) {
                if (item is Button)
                    ;
            }
        }
        
    }
}
