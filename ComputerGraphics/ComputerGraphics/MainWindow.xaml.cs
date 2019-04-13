/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComputerGraphics
{
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
    }

    public partial class MainWindow : Window
    {
        public UserState state = UserState.NONE;
        public Point lastPoint = new Point();

        public MainWindow()
        {
            InitializeComponent();
        }
        



        public void OnBtnCircleClicked(object sender, RoutedEventArgs e)
        {

            state = UserState.BTN_CIRCLE_1ST_CLICK;
        }


        public void Circle(Point p1, Point p2)
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
                    Circle(lastPoint, e.GetPosition(myCanvas));
                    state = UserState.BTN_CIRCLE_1ST_CLICK;
                    break;
                default:
                    break;
            }

        }

        private bool SetPixel(int x, int y) {

            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = System.Windows.Media.Brushes.Blue;
            rect.StrokeThickness = 1;
            rect.Width = 1;
            rect.Height = 1;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            myCanvas.Children.Add(rect);
            return true;

        }

        
    }
}
