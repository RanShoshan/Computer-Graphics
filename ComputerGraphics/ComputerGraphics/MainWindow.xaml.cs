/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ComputerGraphics {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public enum UserState {
        NONE = 0,
        BTN_LINE_1ST_CLICK = 1,
        BTN_LINE_2ST_CLICK = 2,
        BTN_CIRCLE_1ST_CLICK,
        BTN_CIRCLE_2ST_CLICK,
        BTN_BEZIER_1ST_CLICK,
        BTN_BEZIER_2ND_CLICK,
        BTN_BEZIER_3RD_CLICK,
        BTN_BEZIER_4TH_CLICK,
        SCALE,
        STRECH,
        ROTATE,
        MOVE
    }

    public enum PixelStyle {
        DEFAULT = 0,
        BOLD
    }

    public class AnchorPointHelper {
        public Point downPos = new Point(0.0, 0.0);
        public Point upPos = new Point(0.0, 0.0);
    }

    public partial class MainWindow : Window {

        public UserState state = UserState.NONE;
        public Point lastPoint = new Point();
        public Bezier bezier = new Bezier();
        private static string pwd = Directory.GetCurrentDirectory();
        public string tempFilePath = pwd + "\\tempWorkingFilePath.txt";
        private string currentWorkingFile = "";
        public const int STROKE_BOLD = 10;
        private Point anchorPoint = new Point();
        private FileParserUtil parser = new FileParserUtil();
        private readonly string SHAPE_TYPE_LINE_POSTFIX = ": \r\n";
        private readonly char delim = FileParserUtil.delimiter;
        private AnchorPointHelper apHelper = new AnchorPointHelper();
        private int SCALE_UP = 1;
        private int SCALE_DOWN = 0;


        public void WriteToTrackingFile(string str, string shapeKey) {
            string[] full_file = File.ReadAllLines(tempFilePath);
            List<string> lines = new List<string>();
            int lineNum = 0;
            lines.AddRange(full_file);
            for (int i = 0; i < lines.Count; i++) {
                if (lines[i].Contains(shapeKey)) {
                    lineNum = i;
                    break;
                }
            }
            lines.Insert(lineNum + 1, str);
            File.WriteAllLines(tempFilePath, lines.ToArray());
        }

        public void CreatNewTxtFile() {
            File.AppendAllText(tempFilePath, ShapeName.LINE.ToString() + SHAPE_TYPE_LINE_POSTFIX);
            File.AppendAllText(tempFilePath, ShapeName.CIRCLE.ToString() + SHAPE_TYPE_LINE_POSTFIX);
            File.AppendAllText(tempFilePath, ShapeName.BEZIER.ToString() + SHAPE_TYPE_LINE_POSTFIX);
        }

        public MainWindow() {
            InitializeComponent();

            tbBezierNumOfLines.IsEnabled = false;
            Clear();
            InitAnchorPointBtn();


            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;
        }

        private void InitAnchorPointBtn() {
            //anchorPointBtn.BorderBrush = Brushes.White;
            Canvas.SetLeft(anchorPointBtn, 0);
            Canvas.SetTop(anchorPointBtn, 0);


            if (!myCanvas.Children.Contains(anchorPointBtn)) {
                myCanvas.Children.Add(anchorPointBtn);
            }
        }

        private void OnAnchorPointBtnDrag(object s, MouseEventArgs e) {
            Console.WriteLine("OnAnchorPointBtnDrag: " + e.GetPosition(myCanvas));
        }

        private void OnAnchorPointBtnMouseUp(object s, MouseButtonEventArgs e) {
            Console.WriteLine("OnAnchorPointBtnMouseUp: " + e.GetPosition(myCanvas));
            apHelper.upPos = e.GetPosition(myCanvas);
            var dx = apHelper.upPos.X - apHelper.downPos.X;
            var dy = apHelper.upPos.Y - apHelper.downPos.Y;

            var currState = state;
            Clear(false, false);
            parser.ParseFile(tempFilePath);

            switch (currState) {
                case UserState.SCALE:
                    ScaleShapes(dx, dy);
                    break;
                case UserState.STRECH:
                    break;
                case UserState.ROTATE:
                    break;
                case UserState.MOVE:
                    MoveShapes(dx, dy);
                    break;
            }

            DrawShapesFromFile(parser);
        }

        private void MoveShapes(double dx, double dy) {
            foreach (Line line in parser.lineList) {
                line.Move(dx, dy);
            }
            foreach (Circle circle in parser.circleList) {
                circle.Move(dx, dy);
            }
            foreach (Bezier bezier in parser.bezierList) {
                bezier.Move(dx, dy);
            }
        }

        private void OnAnchorPointBtnMouseDown(object s, MouseButtonEventArgs e) {
            Console.WriteLine("OnAnchorPointBtnMouseDown: " + e.GetPosition(myCanvas));
            apHelper.downPos = e.GetPosition(myCanvas);
        }

        private void ScaleShapes(double dx, double dy) {
            int SCALE_DIRECTION = (dy < 0) ? SCALE_UP : SCALE_DOWN;

            foreach (Line line in parser.lineList) {
                var scaleValue = GetScaleValue(SCALE_DIRECTION, dy, line.pt2.Y, line.pt1.Y);

                LineCalibrated lc = new LineCalibrated(line);
                MultPointsBy(ref lc.calibrated.pt1, ref lc.calibrated.pt2, scaleValue);
                lc.FixCalibrationOffset();
                line.pt1.X = lc.calibrated.pt1.X;
                line.pt1.Y = lc.calibrated.pt1.Y;
                line.pt2.X = lc.calibrated.pt2.X;
                line.pt2.Y = lc.calibrated.pt2.Y;
            }
            foreach (Circle circle in parser.circleList) {
                var scaleValue = GetScaleValue(SCALE_DIRECTION, dy, circle.pt2.Y, circle.pt1.Y);
                CircleCalibrated cc = new CircleCalibrated(circle);
                MultPointsBy(ref cc.calibrated.pt1, ref cc.calibrated.pt2, scaleValue);
                cc.FixCalibrationOffset();
                circle.pt1.X = cc.calibrated.pt1.X;
                circle.pt1.Y = cc.calibrated.pt1.Y;
                circle.pt2.X = cc.calibrated.pt2.X;
                circle.pt2.Y = cc.calibrated.pt2.Y;
            }
            foreach (Bezier bezier in parser.bezierList) {
            }
            //var linearDistance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

        }

        private double GetScaleValue(int SCALE_DIRECTION, double dy, double y2, double y1) {
            var scaleValue = SCALE_DIRECTION + Math.Abs(dy) / Math.Abs(y2 - y1);
            if (SCALE_DIRECTION == SCALE_DOWN) {
                scaleValue = 1 - scaleValue;
            }
            return scaleValue;
        }

        private void MultPointsBy(ref Point p1, ref Point p2, double scaleValue) {
            p1.X *= scaleValue;
            p1.Y *= scaleValue;
            p2.X *= scaleValue;
            p2.Y *= scaleValue;
        }

        public void OnBtnClearClicked(object sender, RoutedEventArgs e) {
            Clear();
        }

        public void Clear(bool clearCache = true, bool resetState = true) {
            if (resetState) {
                state = UserState.NONE;
            }
            ToggleOffAllButtons(null, resetState);
            myCanvas.Children.Clear();
            if (myCanvas.Children.Contains(anchorPointBtn)) {
                myCanvas.Children.Remove(anchorPointBtn);
            }
            myCanvas.Children.Add(anchorPointBtn);

            if (clearCache) {
                parser.ClearCache();
                File.Delete(tempFilePath);
                CreatNewTxtFile();
            }


            anchorPoint.X = anchorPoint.Y = 0;
        }

        public void OnBtnCircleClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnCircle);
            state = UserState.BTN_CIRCLE_1ST_CLICK;
        }

        public void OnBtnBrushClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnBrush);
        }

        public void OnBtnPaintcanClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnPaintcan);
        }

        public void DrawCircle(Circle obj) {
            DrawCircle(obj.pt1, obj.pt2);
        }

        public void DrawCircle(Point p1, Point p2) {
            int xCenter = Convert.ToInt32(p2.X);
            int yCenter = Convert.ToInt32(p2.Y);
            int xr = Convert.ToInt32(p1.X);
            int yr = Convert.ToInt32(p1.Y);

            var a = (xCenter - xr);
            var b = (yCenter - yr);
            var r = Math.Sqrt(a * a + b * b);

            for (double i = 0.0; i < 360.0; i += 0.1) {
                double angle = i * System.Math.PI / 180;
                int x = (int)(xr + r * System.Math.Cos(angle));
                int y = (int)(yr + r * System.Math.Sin(angle));
                SetPixel(x, y);
            }

        }

        public void OnBtnLineClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnLine);
            state = UserState.BTN_LINE_1ST_CLICK;
        }

        public void OnBtnBezierClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnBizier);
            state = UserState.BTN_BEZIER_1ST_CLICK;
            tbBezierNumOfLines.IsEnabled = true;
        }

        private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        public void DrawLine(Line line) {
            if (line != null)
                DrawLine(line.pt1, line.pt2);
        }

        public void DrawLine(Point p1, Point p2) {
            int x0 = Convert.ToInt32(p1.X);
            int y0 = Convert.ToInt32(p1.Y);
            int x1 = Convert.ToInt32(p2.X);
            int y1 = Convert.ToInt32(p2.Y);
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
            if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
            int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

            for (int x = x0; x <= x1; ++x) {
                if (!(steep ? SetPixel(y, x) : SetPixel(x, y))) return;
                err = err - dY;
                if (err < 0) { y += ystep; err += dX; }
            }
        }

        public string PointToString(Point p) {
            return p.X.ToString() + ',' + p.Y.ToString();
        }

        public void OnCanvasMouseDown(object sender, MouseButtonEventArgs e) {
            Point p = e.GetPosition(myCanvas);

            switch (state) {
                case UserState.NONE:
                    break;
                case UserState.BTN_LINE_1ST_CLICK:
                    lastPoint = p;
                    state = UserState.BTN_LINE_2ST_CLICK;
                    break;
                case UserState.BTN_LINE_2ST_CLICK:
                    DrawLine(lastPoint, p);
                    WriteToTrackingFile(PointToString(lastPoint) + delim + PointToString(p), ShapeName.LINE.ToString());
                    state = UserState.BTN_LINE_1ST_CLICK;
                    break;

                case UserState.BTN_CIRCLE_1ST_CLICK:
                    lastPoint = p;
                    state = UserState.BTN_CIRCLE_2ST_CLICK;
                    break;
                case UserState.BTN_CIRCLE_2ST_CLICK:
                    DrawCircle(lastPoint, p);
                    WriteToTrackingFile(PointToString(lastPoint) + delim + PointToString(p), ShapeName.CIRCLE.ToString());
                    state = UserState.BTN_CIRCLE_1ST_CLICK;
                    break;

                case UserState.BTN_BEZIER_1ST_CLICK:
                    state = UserState.BTN_BEZIER_2ND_CLICK;
                    bezier.cp1 = p;
                    SetPixel(Convert.ToInt32(bezier.cp1.X), Convert.ToInt32(bezier.cp1.Y), PixelStyle.BOLD, Brushes.Aqua, false);
                    break;
                case UserState.BTN_BEZIER_2ND_CLICK:
                    state = UserState.BTN_BEZIER_3RD_CLICK;
                    bezier.cp2 = p;
                    SetPixel(Convert.ToInt32(bezier.cp2.X), Convert.ToInt32(bezier.cp2.Y), PixelStyle.BOLD, Brushes.Aqua, false);
                    break;
                case UserState.BTN_BEZIER_3RD_CLICK:
                    state = UserState.BTN_BEZIER_4TH_CLICK;
                    bezier.cp3 = p;
                    SetPixel(Convert.ToInt32(bezier.cp3.X), Convert.ToInt32(bezier.cp3.Y), PixelStyle.BOLD, Brushes.Aqua, false);
                    break;
                case UserState.BTN_BEZIER_4TH_CLICK:
                    bezier.cp4 = p;
                    SetPixel(Convert.ToInt32(bezier.cp4.X), Convert.ToInt32(bezier.cp4.Y), PixelStyle.BOLD, Brushes.Aqua, false);
                    DrawBezierCurve(bezier, tbBezierNumOfLines.Text);
                    WriteToTrackingFile(
                        PointToString(bezier.cp1) + delim + PointToString(bezier.cp2) + delim +
                        PointToString(bezier.cp3) + delim + PointToString(bezier.cp4), ShapeName.BEZIER.ToString());
                    state = UserState.BTN_BEZIER_1ST_CLICK;
                    break;
                default:
                    break;
            }
            //UpdateAnchorPoint(p);
        }

        public void DrawBezierCurve(Bezier b, string smoothingRate) {
            var lineStart = new Point(0, 0);
            var lineEnd = new Point(0, 0);
            var bezierPoints = new List<Point>();
            double smoothingrate = 1.0 / Convert.ToDouble(smoothingRate);

            RemoveBezierGuidePoints(b);

            for (double t = 0.0; t <= 1.0; t = t + smoothingrate) {
                var put_x = Math.Pow(1 - t, 3) * b.cp1.X + 3 * t * Math.Pow(1 - t, 2) * b.cp2.X + 3 * t * t * (1 - t) * b.cp3.X + Math.Pow(t, 3) * b.cp4.X; // Formula to draw curve
                var put_y = Math.Pow(1 - t, 3) * b.cp1.Y + 3 * t * Math.Pow(1 - t, 2) * b.cp2.Y + 3 * t * t * (1 - t) * b.cp3.Y + Math.Pow(t, 3) * b.cp4.Y;
                bezierPoints.Add(new Point(put_x, put_y));
            }

            for (int i = 0; i < bezierPoints.Count - 1; i++) {
                DrawLine(bezierPoints[i], bezierPoints[i + 1]);
            }
            DrawLine(bezierPoints[bezierPoints.Count - 1], b.cp4);
        }

        private void RemoveBezierGuidePoints(Bezier b) {
            SetPixel(Convert.ToInt32(b.cp1.X), Convert.ToInt32(b.cp1.Y), PixelStyle.BOLD, Brushes.White, false);
            SetPixel(Convert.ToInt32(b.cp2.X), Convert.ToInt32(b.cp2.Y), PixelStyle.BOLD, Brushes.White, false);
            SetPixel(Convert.ToInt32(b.cp3.X), Convert.ToInt32(b.cp3.Y), PixelStyle.BOLD, Brushes.White, false);
            SetPixel(Convert.ToInt32(b.cp4.X), Convert.ToInt32(b.cp4.Y), PixelStyle.BOLD, Brushes.White, false);
        }

        private bool SetPixel(int x, int y, PixelStyle style = PixelStyle.DEFAULT, Brush color = null, bool updateAnchor = true) {

            if (updateAnchor) {
                Point p = new Point(x, y);
                UpdateAnchorPoint(p);
            }
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = color ?? Brushes.Blue;
            if (style == PixelStyle.BOLD) {
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

        public void ToggleOffAllButtons(ToggleButton activeBtn = null, bool hideAnchor = true) {

            foreach (var item in mainToolbar.Items) {
                if (item is ToggleButton) {
                    ((ToggleButton)item).IsChecked = false;
                }
            }

            if (activeBtn != null) {
                activeBtn.IsChecked = true;
            }

            if (hideAnchor) {
                ShowAnchorPoint(false);
            }
        }

        public void OnBtnSaveClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons();
            SaveFile(currentWorkingFile);
        }

        public void OnBtnLoadClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons();

            var ofd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "Text Files (*.txt)|*.txt"
            };
            var result = ofd.ShowDialog();
            if (result == true) {
                currentWorkingFile = ofd.FileName;
                parser.ParseFile(currentWorkingFile);
                DrawShapesFromFile(parser);
            }
        }

        private void DrawShapesFromFile(FileParserUtil parser) {
            File.Delete(tempFilePath);
            CreatNewTxtFile();

            DrawLines(parser.lineList);
            DrawCircles(parser.circleList);
            DrawBezierCurves(parser.bezierList);
        }

        private void DrawBezierCurves(List<Bezier> bezierList) {
            foreach (var obj in bezierList) {
                DrawBezierCurve(obj, Bezier.DEFAULT_SMOOTHING_RATE);
                WriteToTrackingFile(
                    PointToString(obj.cp1) + delim +
                    PointToString(obj.cp2) + delim +
                    PointToString(obj.cp3) + delim +
                    PointToString(obj.cp4),
                    ShapeName.BEZIER.ToString());
            }
        }

        private void DrawCircles(List<Circle> circleList) {
            foreach (var obj in circleList) {
                DrawCircle(obj);
                WriteToTrackingFile(
                    PointToString(obj.pt1) +
                    delim +
                    PointToString(obj.pt2),
                    ShapeName.CIRCLE.ToString());
            }
        }

        private void DrawLines(List<Line> lineList) {
            foreach (var obj in lineList) {
                DrawLine(obj);
                WriteToTrackingFile(
                    PointToString(obj.pt1) +
                    delim +
                    PointToString(obj.pt2),
                    ShapeName.LINE.ToString());
            }
        }

        private void SaveFile(string fileName) {
            if (File.Exists(tempFilePath) && File.Exists(currentWorkingFile)) {
                File.Delete(currentWorkingFile);
                File.Copy(tempFilePath, currentWorkingFile);
            }
        }
        public void OnBtnScaleClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnScale);
            state = UserState.SCALE;
            ShowAnchorPoint();
        }

        public void OnBtnStrechClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnStrech);
            state = UserState.STRECH;
            ShowAnchorPoint();
        }

        public void OnBtnRotateClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnRotate);
            state = UserState.ROTATE;
            ShowAnchorPoint();
        }

        private void UpdateAnchorPoint(Point p) {
            anchorPoint.X = Math.Max(anchorPoint.X, p.X);
            anchorPoint.Y = anchorPoint.Y > 0 ? Math.Min(anchorPoint.Y, p.Y) : p.Y;
            //var currX = Canvas.GetLeft(anchorPointBtn);
            //var currY = Canvas.GetTop(anchorPointBtn);
            Canvas.SetLeft(anchorPointBtn, anchorPoint.X);
            Canvas.SetTop(anchorPointBtn, anchorPoint.Y);
            Console.WriteLine("p = " + p.X + "," + p.Y);
            Console.WriteLine("anchorPoint = " + anchorPoint.X + "," + anchorPoint.Y);
        }

        private void ShowAnchorPoint(bool show = true) {
            if (show == true) {
                //SetPixel(Convert.ToInt32(anchorPoint.X), Convert.ToInt32(anchorPoint.Y), PixelStyle.BOLD, Brushes.Orange, false);
                anchorPointBtn.Visibility = Visibility.Visible;
            }
            else {
                anchorPointBtn.Visibility = Visibility.Hidden;
            }

        }

        public void OnBtnMoveClicked(object sender, RoutedEventArgs e) {
            state = UserState.MOVE;
            ToggleOffAllButtons(btnMove);
            ShowAnchorPoint();
        }

    }
}
