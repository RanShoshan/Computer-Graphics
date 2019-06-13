/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
        SCALE_UP,
        SCALE_DOWN,
        STRECH,
        ROTATE,
        MOVE,
        MIRROR
    }

    public enum MirrorDirection {
        X,
        Y
    }
    public enum PixelStyle {
        DEFAULT = 0,
        BOLD
    }

    public enum ProjectionType {
        PARALLEL,
        OBLIQUE,
        PERSPECTIVE
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
        Point centerPoint = new Point();

        //keep track of new shapes (points) added to the canvas
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
            File.AppendAllText(tempFilePath, ShapeName.VERTEX.ToString() + SHAPE_TYPE_LINE_POSTFIX);
            File.AppendAllText(tempFilePath, ShapeName.POLYGON.ToString() + SHAPE_TYPE_LINE_POSTFIX);
        }

        public MainWindow() {
            InitializeComponent();

            tbBezierNumOfLines.IsEnabled = false;
            Clear();
            InitAnchorPointBtn();

            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;
        }

        //the anchor point btn is used to display the top right btn that is to be dragged on Transformation:
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
            var angle = Double.Parse(tbRotateDegrees.Text);

            var currState = state;
            Clear(false, false);
            parser.ParseFile(tempFilePath);

            switch (currState) {
                case UserState.STRECH:
                    break;
                case UserState.ROTATE:
                    RotateShapes(angle);
                    break;
                case UserState.MOVE:
                    MoveShapes(dx, dy);
                    break;
            }

            DrawShapesFromFile(parser);
        }

        //Mirroring a shape on x or y axis:
        private void MirrorShapes(MirrorDirection direction) {
            state = UserState.MIRROR;
            ToggleOffAllButtons(direction == MirrorDirection .X ? btnMirrorX : btnMirrorY);
            Clear(false);

            centerPoint.X = myCanvas.ActualWidth / 2;
            centerPoint.Y = myCanvas.ActualHeight / 2;
            
            foreach (MyLine line in parser.lineList) {
                line.Mirror(centerPoint, direction);
            }
            foreach (Circle circle in parser.circleList) {
                circle.Mirror(centerPoint, direction);
            }
            foreach (Bezier bezier in parser.bezierList) {
                bezier.Mirror(centerPoint, direction);
            }

            DrawShapesFromFile(parser);

        }

        //Move shapes on the canvas:
        private void MoveShapes(double dx, double dy) {
            foreach (MyLine line in parser.lineList) {
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

        //Scaling algorithm - scaleup value = 1.075 for every click, scaledown value = 0.925:
        private void ScaleShapes(double scaleVal = 0.0) {
            var defaultScaleValUp = 1.075;
            var defaultScaleValDown = 0.925;

            if (scaleVal == 0.0) {
                scaleVal = defaultScaleValUp;
            }

            if (state == UserState.SCALE_DOWN) {
                scaleVal = defaultScaleValDown;
            }

            foreach (MyLine line in parser.lineList)
                line.Scale(scaleVal);
            foreach (Circle circle in parser.circleList)
                circle.Scale(scaleVal);
            foreach (Bezier b in parser.bezierList)
                b.Scale(scaleVal);
            Clear(false);
            //CenterShapes();
            //DrawShapesFromFile(parser);
        }

        //Used to find top left and bottom right corners of all shapes, calculate their midpoint
        //and add (or subtruct) its offset from the canvas middle X,Y position in order to center
        //all shapes to the center of the canvas
        private void CenterShapes() {
            var midPoint = CalculateMiddlePoint(parser);
            var centerXOffset = Math.Abs(midPoint.X - (Width / 2));
            var centerYOffset = Math.Abs(midPoint.Y - (Height / 2));

            if (midPoint.X > (Width / 2)) {
                centerXOffset *= (-1);
            }
            if (midPoint.Y > (Height/ 2)) {
                centerYOffset *= (-1);
            }

            MoveShapes(centerXOffset, centerYOffset);
            Console.WriteLine("centerXOffset = " + centerXOffset);
            Console.WriteLine("centerYOffset = " + centerYOffset);

        }

        //get the middle point of all shapes (as a "batch")
        private Point CalculateMiddlePoint(FileParserUtil parser) {
            MyLine baseLine = parser.lineList[0];
            Point topLeft = new Point(baseLine.pt1.X, baseLine.pt1.Y);
            Point botRight = new Point(baseLine.pt1.X, baseLine.pt1.Y);
            foreach (var line in parser.lineList) {
                topLeft.X = Math.Min(topLeft.X, ShapesHelper.GetMinX(line));
                topLeft.Y = Math.Min(topLeft.Y, ShapesHelper.GetMinY(line));
                botRight.X = Math.Max(botRight.X, ShapesHelper.GetMaxX(line));
                botRight.Y = Math.Max(botRight.Y, ShapesHelper.GetMaxY(line));
            }
            foreach (var circle in parser.circleList) {
                topLeft.X = Math.Min(topLeft.X, ShapesHelper.GetMinX(circle));
                topLeft.Y = Math.Min(topLeft.Y, ShapesHelper.GetMinY(circle));
                botRight.X = Math.Max(botRight.X, ShapesHelper.GetMaxX(circle));
                botRight.Y = Math.Max(botRight.Y, ShapesHelper.GetMaxY(circle));
            }
            foreach (var bezier in parser.bezierList) {
                topLeft.X = Math.Min(topLeft.X, ShapesHelper.GetMinX(bezier));
                topLeft.Y = Math.Min(topLeft.Y, ShapesHelper.GetMinY(bezier));
                botRight.X = Math.Max(botRight.X, ShapesHelper.GetMaxX(bezier));
                botRight.Y = Math.Max(botRight.Y, ShapesHelper.GetMaxY(bezier));
            }

            Console.WriteLine("topLeft = " + topLeft.X + "," + topLeft.Y);
            Console.WriteLine("botRight = " + botRight.X + "," + botRight.Y);
            
            var midPoint = new Point(Math.Abs(botRight.X + topLeft.X) / 2, Math.Abs(botRight.Y + topLeft.Y) / 2);
            Console.WriteLine("midPoint = " + midPoint.X + "," + midPoint.Y);
            return midPoint;
        }
        
        //Scaling logic - multiply by a constant
        private void MultPointsBy(ref Point p1, ref Point p2, double scaleValue) {
            p1.X *= scaleValue;
            p1.Y *= scaleValue;
            p2.X *= scaleValue;
            p2.Y *= scaleValue;
        }

        public void OnBtnClearClicked(object sender, RoutedEventArgs e) {
            Clear();
        }

        //Clear canvas, free memory, reset state of buttons, clear temp tracking file
        public void Clear(bool clearCache = true, bool resetState = true) {
            if (resetState) {
                state = UserState.NONE;
            }
            ToggleOffAllButtons(null, resetState);
            ReattachHelperButtons();

            if (clearCache) {
                parser.ClearCache();
                File.Delete(tempFilePath);
                CreatNewTxtFile();
            }

            anchorPoint.X = anchorPoint.Y = 0;
            GC.Collect(); //does it really help us getting rid of non-referenced memory??? xD
        }

        //Gui reattachments
        private void ReattachHelperButtons() {
            myCanvas.Children.Clear();
            ReattachAnchorPointBtn();
            ReattachHelpWindow();
        }

        //Gui reattachments
        private void ReattachHelpWindow() {
            if (myCanvas.Children.Contains(helpWindow)) {
                myCanvas.Children.Remove(helpWindow);
            }
            myCanvas.Children.Add(helpWindow);
            helpWindow.Text = MenuHelper.MENU_TEXT;
        }

        //Gui reattachments
        private void ReattachAnchorPointBtn() {
            if (myCanvas.Children.Contains(anchorPointBtn)) {
                myCanvas.Children.Remove(anchorPointBtn);
            }
            myCanvas.Children.Add(anchorPointBtn);
        }

        public void OnBtnCircleClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnCircle);
            state = UserState.BTN_CIRCLE_1ST_CLICK;
        }

        public void DrawCircle(Circle obj) {
            DrawCircle(obj.pt1, obj.pt2);
        }

        public void DrawCircle(Point p1, Point p2) {
            var radius = Math.Max(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
            Ellipse circle = new Ellipse() {
                Width = radius*2,
                Height = radius*2,
                Stroke = Brushes.Blue,
                StrokeThickness = 1
            };

            myCanvas.Children.Add(circle);

            UpdateAnchorPoint(new Point(p1.X + radius, p1.Y - radius));
            circle.SetValue(Canvas.LeftProperty, (double)p1.X- radius);
            circle.SetValue(Canvas.TopProperty, (double)p1.Y - radius);
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

        public void DrawLine(MyLine line) {
            if (line != null)
                DrawLine(line.pt1, line.pt2);
        }

        public void DrawLine(Point p1, Point p2) {
            
            var line = new Line {
                Stroke = Brushes.Blue,
                X1 = p1.X,
                X2 = p2.X,
                Y1 = p1.Y,
                Y2 = p2.Y,
                StrokeThickness = 1
            };
            myCanvas.Children.Add(line);
            UpdateAnchorPoint(p1);
            UpdateAnchorPoint(p2);
        }

        //conversion function
        public string PointToString(Point p) {
            return p.X.ToString() + ',' + p.Y.ToString();
        }

        //handle canvas events
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
                    break;
                case UserState.BTN_BEZIER_2ND_CLICK:
                    state = UserState.BTN_BEZIER_3RD_CLICK;
                    bezier.cp2 = p;
                    break;
                case UserState.BTN_BEZIER_3RD_CLICK:
                    state = UserState.BTN_BEZIER_4TH_CLICK;
                    bezier.cp3 = p;
                    break;
                case UserState.BTN_BEZIER_4TH_CLICK:
                    bezier.cp4 = p;
                    DrawBezierCurve(bezier, tbBezierNumOfLines.Text);
                    WriteToTrackingFile(
                        PointToString(bezier.cp1) + delim + PointToString(bezier.cp2) + delim +
                        PointToString(bezier.cp3) + delim + PointToString(bezier.cp4), ShapeName.BEZIER.ToString());
                    state = UserState.BTN_BEZIER_1ST_CLICK;
                    break;
                default:
                    break;
            }
        }

        public void DrawBezierCurve(Bezier b, string smoothingRate) {
            var lineStart = new Point(0, 0);
            var lineEnd = new Point(0, 0);
            var bezierPoints = new List<Point>();
            double smoothingrate = 1.0 / Convert.ToDouble(smoothingRate);

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
        
        //remove button toggles
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
                DrawProjection();
            }
        }

        private void DrawProjection() {
            //get the projection type from the gui radio buttons
            var type = ProjectionType.PARALLEL;

            switch (type) {
                case ProjectionType.PARALLEL:
                    DrawParallel();
                    break;
                case ProjectionType.OBLIQUE:
                    DrawOblique();
                    break;
                case ProjectionType.PERSPECTIVE:
                    DrawPerspective();
                    break;

            }
        }

        private void DrawPerspective() {
            
        }

        private void DrawOblique() {
            
        }

        private void DrawParallel() {
            
        }

        //draw all shapes from the current working file
        private void DrawShapesFromFile(FileParserUtil parser) {
            File.Delete(tempFilePath);
            CreatNewTxtFile();
            DrawPolygons(parser.polygonList);
        }

        private void DrawPolygons(List<MyPolygon> polygonList) {
            foreach (var polygon in polygonList) {
                var newPoly = new Polygon {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Points = new PointCollection()
                };

                //create new point collection with calculated offset before drawing on canvas:
                for(int i=0; i < polygon.poly.Points.Count ; i++) {
                    newPoly.Points.Add(new Point(
                        polygon.poly.Points[i].X + Width / 2,
                        polygon.poly.Points[i].Y + Height/ 2
                        ));
                }

                myCanvas.Children.Add(newPoly);
            }
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

        private void DrawLines(List<MyLine> lineList) {
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


        public void OnBtnApplyRotationClicked(object sender, RoutedEventArgs e) {
            
            foreach (RadioButton radioBtn in RotationAngleGrp.Children) {
            }
        }
        
        public void OnBtnApplyTransformationClicked(object sender, RoutedEventArgs e) {

        }

        public void OnBtnApplyScalingClicked(object sender, RoutedEventArgs e) {
            
        }

        private void OnRotationSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var slider = sender as Slider;
            //double value = Math.Round(slider.Value, MAX_DIGITS_AFTER_F_POINT);

            // ... Set Window Title.
            RotationValueTb.Text = slider.Value.ToString();
        }

        //validate legal angle value on input entered
        private void OnRotationValueInputTextChanged(object sender, TextChangedEventArgs args) {
            var MAX_ANGLE_DIGITS = 3;
            var textBox = sender as TextBox;
            var illegalInputMsg = "Input should only contain \nnumbers between -360 and 360!";
            Double angleValue = 0.0;

            Regex negRgx = new Regex(@"^-[0-9]+$");
            Regex posRgx = new Regex(@"^[0-9]+$");
            
            Match negMatch = negRgx.Match(RotationValueTb.Text);
            Match posMatch = posRgx.Match(RotationValueTb.Text);

            DisplayIllegalValueNotificiation(null, false);

            if (textBox.Text.Contains("-")) {
                MAX_ANGLE_DIGITS++;
            }
            if (textBox.Text.Length > MAX_ANGLE_DIGITS) {
                textBox.Text = textBox.Text.Substring(0, MAX_ANGLE_DIGITS);
                return;
            }

            //negative angle
            if (negMatch.Success) {
                Console.WriteLine("negMatch.");
                angleValue = Double.Parse(RotationValueTb.Text.Replace("-", ""));
                if (isInValidRange(angleValue)) {
                    RotationSlider.Value = angleValue * -1;
                }
                else {
                    DisplayIllegalValueNotificiation(illegalInputMsg);
                    textBox.Text = "";
                }
            }
            //positive angle
            else if (posMatch.Success) {
                Console.WriteLine("posMatch.");
                angleValue = Double.Parse(RotationValueTb.Text);
                if (isInValidRange(angleValue)) {
                    RotationSlider.Value = angleValue;
                }
                else {
                    DisplayIllegalValueNotificiation(illegalInputMsg);
                    textBox.Text = "";
                }
            }
            //illegal input
            else {
                if (textBox.Text != "-") {
                    Console.WriteLine("illegal input");
                    textBox.Text = "";
                    DisplayIllegalValueNotificiation(illegalInputMsg);
                }
            }
            return;
        }

        private void DisplayIllegalValueNotificiation(string msg = "", bool display = true) {
            if (UserNotificationBorder != null && UserNotification != null) {
                UserNotificationBorder.Visibility = display ? Visibility.Visible : Visibility.Hidden;
                UserNotification.Text = display ? msg : "";
            }
        }

        private bool isInValidRange(double angleValue) {
            return angleValue >= 0 && angleValue <= 360;
        }

        public void OnBtnScaleUpClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons();
            state = UserState.SCALE_UP;
            ScaleShapes();
            DrawShapesFromFile(parser);
        }

        public void OnBtnScaleDownClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons();
            state = UserState.SCALE_DOWN;
            ScaleShapes();
            DrawShapesFromFile(parser);
        }

        public void OnBtnStrechXClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnStrechX);
            state = UserState.STRECH;
            ShowAnchorPoint();
        }

        public void OnBtnStrechYClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnStrechY);
            state = UserState.STRECH;
            ShowAnchorPoint();
        }

        public void OnBtnRotateClicked(object sender, RoutedEventArgs e) {
            ToggleOffAllButtons(btnRotate);
            state = UserState.ROTATE;
            ShowAnchorPoint();
        }

        //update top right corner point of all shapes, used to calcualte position for our anchor button
        private void UpdateAnchorPoint(Point p) {
            anchorPoint.X = Math.Max(anchorPoint.X, p.X);
            anchorPoint.Y = anchorPoint.Y > 0 ? Math.Min(anchorPoint.Y, p.Y) : p.Y;
            Canvas.SetLeft(anchorPointBtn, anchorPoint.X);
            Canvas.SetTop(anchorPointBtn, anchorPoint.Y);
        }

        private void ShowAnchorPoint(bool show = true) {
            if (show == true) {
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

        public void OnMirrorBtnXClick(object sender, RoutedEventArgs e) {
            MirrorShapes(MirrorDirection.X);
        }

        public void OnMirrorBtnYClick(object sender, RoutedEventArgs e) {
            MirrorShapes(MirrorDirection.Y);
        }

        public void OnBtnHelpClicked(object sender, RoutedEventArgs e) {
            helpWindow.Visibility = helpWindow.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }

        
        //rotate shapes around center point
        private void RotateShapes(double angle) {

            foreach (MyLine line in parser.lineList) {
                /* We are using default center point (center of screen). But we could also calculate 
                 * the line's as middle point:
                 * lineCenterPoint.X/Y = (line.pt1.X/Y + line.pt2.X/Y) / 2;
                 */

                //rotate points around center screen:
                line.pt1 = RotatePoint(line.pt1, angle);
                line.pt2 = RotatePoint(line.pt2, angle);

            }
            foreach (Circle circle in parser.circleList) {
                //rotate points around center screen:
                circle.pt1 = RotatePoint(circle.pt1, angle);
                circle.pt2 = RotatePoint(circle.pt2, angle);
            }
            foreach (Bezier bezier in parser.bezierList) {
                /* We are using default center point (center of screen). But we could also calculate 
                 * middle point of the bezier curve as as:
                 * lineCenterPoint.X/Y = (bezier.cp1.X/Y + bezier.cp2.X/Y + bezier.cp3.X/Y + bezier.cp4.X/Y) / 4;
                 */
                
                //rotate points around center screen:
                bezier.cp1 = RotatePoint(bezier.cp1, angle);
                bezier.cp2 = RotatePoint(bezier.cp2, angle);
                bezier.cp3 = RotatePoint(bezier.cp3, angle);
                bezier.cp4 = RotatePoint(bezier.cp4, angle);
            }
        }

        
        internal Point RotatePoint(Point pointToRotate, double angleInDegrees) {
            centerPoint.X = myCanvas.ActualWidth / 2;
            centerPoint.Y = myCanvas.ActualHeight / 2;
            return RotatePoint(pointToRotate, centerPoint, angleInDegrees);
        }

        //Rotation algorithm for a single point according to angle input from GUI
        internal Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees) {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point {
                X = (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y = (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        private void RotationValueTb_TextChanged(object sender, TextChangedEventArgs e) {

        }
    }
}
