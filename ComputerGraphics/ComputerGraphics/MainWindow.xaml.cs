/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ComputerGraphics {
    
    public enum Axis {
        X,Y,Z
    }

    public enum ProjectionType {
        ORTHOGRAPHIC,
        OBLIQUE,
        PERSPECTIVE
    }
    
    public partial class MainWindow : Window {

        private static string pwd = Directory.GetCurrentDirectory();
        public string tempFilePath = pwd + "\\tempWorkingFilePath.txt";
        private string currentWorkingFile = "";
        public const int STROKE_BOLD = 10;
        private FileParserUtil parser = new FileParserUtil();
        private readonly string SHAPE_TYPE_LINE_POSTFIX = ": \r\n";
        private readonly char delim = FileParserUtil.delimiter;
        private static readonly string CONFIG_FILENAME = "..\\..\\config7.txt";

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
            Clear();
            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;
            PublishOffset();
            LoadFile(CONFIG_FILENAME);
        }

        private void PublishOffset() {
            Transformations.screen = new Point(Width, Height);
        }

        public void OnBtnClearClicked(object sender, RoutedEventArgs e) {
            Clear();
        }

        //Clear canvas, free memory, reset state of buttons, clear temp tracking file
        public void Clear(bool clearCache = true, bool resetState = true) {
           
            ToggleOffAllButtons(null, resetState);
            ReattachHelperButtons();

            if (clearCache) {
                parser.ClearCache();
                File.Delete(tempFilePath);
                CreatNewTxtFile();
            }

            GC.Collect(); //does it really help us getting rid of non-referenced memory??? xD
        }

        //Gui reattachments
        private void ReattachHelperButtons() {
            myCanvas.Children.Clear();
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

        //conversion function
        public string PointToString(Point p) {
            return p.X.ToString() + ',' + p.Y.ToString();
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

        }

        private void LoadFile(string fileName) {
            currentWorkingFile = fileName;
            parser.ParseFile(currentWorkingFile);
            DrawPolygons(parser.polygonList);
            DrawShapesFromFile(parser);
        }

        private void DrawProjection() {

            //get the projection type from the gui radio buttons
            var type = GetProjectionType();

            //clear canvas
            Clear();

            //reload original positions
            parser.ParseFile(currentWorkingFile);

            List<MyPolygon> projectedPolygons = new List<MyPolygon>();

            for (int i = 0; i < parser.polygonList.Count; i++) {
                projectedPolygons.Add(parser.polygonList[i].PerformProjection(GetProjectionType()));
            }

            DrawPolygons(projectedPolygons);

        }

        private ProjectionType GetProjectionType() {
            foreach(RadioButton type in ProjectionRadioGrpPanel.Children) {
                if(type.IsChecked == true) {
                    if(type.Content.ToString() == "Orthographic") {
                        return ProjectionType.ORTHOGRAPHIC;
                    }
                    if (type.Content.ToString() == "Oblique") {
                        return ProjectionType.OBLIQUE;
                    }
                    else { 
                        return ProjectionType.PERSPECTIVE;
                    }
                }
            }
            return ProjectionType.ORTHOGRAPHIC;
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

                //draw polygon on canvas:
                myCanvas.Children.Add(newPoly);
            }            
        }

        public void OnBtnApplyRotationClicked(object sender, RoutedEventArgs e) {
            
            //clear canvas
            Clear(false);

            var axis = Axis.X;
            var angle = Double.Parse(RotationValueTb.Text);

            foreach (RadioButton radioBtn in RotationAngleGrpPanel.Children) {
                if (radioBtn.IsChecked == true) {
                    axis = GetAxis(radioBtn);
                    break;
                }
            }

            List<MyPolygon> projectedPolygons = new List<MyPolygon>();
            for (int i = 0; i < parser.polygonList.Count; i++) {
                parser.polygonList[i].Rotate(axis, angle);
                projectedPolygons.Add(parser.polygonList[i].PerformProjection(GetProjectionType()));
            }
            DrawPolygons(projectedPolygons);

        }

        private Axis GetAxis(RadioButton radioBtn) {
            if (radioBtn.Content.ToString() == "X") {
                return Axis.X;
            }
            else if (radioBtn.Content.ToString() == "Y") {
                return Axis.Y;
            }
            else {
                return Axis.Z;
            }
        }

        public void OnBtnApplyTransitionClicked(object sender, RoutedEventArgs e) {

        }

        public void OnBtnApplyScalingClicked(object sender, RoutedEventArgs e) {
            //clear canvas
            Clear(false);

            var scaleValue = Double.Parse(ScalingValueTb.Text);

            for (int i = 0; i < parser.polygonList.Count; i++) {
                parser.polygonList[i].Scale(scaleValue);
            }

            if (ShapesOutOfBordersAfterScaling()) {
                ScaleShapesBackToRecentSize();
            }

            DrawPolygons(parser.polygonList);
        }

        private bool ShapesOutOfBordersAfterScaling() {
            //todo: validate vertex are in canvas border
            return false;
        }

        private void ScaleShapesBackToRecentSize() {
            //todo: scale back to recent dims if needed
        }


        private void OnRotationSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            var slider = sender as Slider;
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
                if (IsInValidRange(angleValue)) {
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
                if (IsInValidRange(angleValue)) {
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

        private bool IsInValidRange(double angleValue) {
            return angleValue >= 0 && angleValue <= 360;
        }

        public void OnBtnHelpClicked(object sender, RoutedEventArgs e) {
            helpWindow.Visibility = helpWindow.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }

        private void OnProjectionChanged(object sender, RoutedEventArgs e) {
            Console.WriteLine("OnProjectionChanged");
            if(myCanvas != null) {
                DrawProjection();
                OnBtnApplyScalingClicked(null, null);
            }
        }

    }
}
