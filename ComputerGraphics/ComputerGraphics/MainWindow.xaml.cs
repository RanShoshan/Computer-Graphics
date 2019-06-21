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
        public const int STROKE_BOLD = 10;
        private FileParserUtil parser = new FileParserUtil();
        private readonly char delim = FileParserUtil.delimiter;
        private bool showInvisibleSurface = true;
        private double totalScaledValue = 1.0;

        public MainWindow() {
            InitializeComponent();
            Clear();
            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;
            PublishOffset();
            InitPolygons();
        }

        private void PublishOffset() {
            Transformations.screen = new Point(Width, Height);
        }

        public void OnBtnClearClicked(object sender, RoutedEventArgs e) {
            Clear();
        }

        //Clear canvas, free memory, reset state of buttons, clear temp tracking file
        public void Clear(bool clearCache = true, bool resetState = true) {
            DisplayIllegalValueNotificiation("", false);
            ToggleOffAllButtons(null, resetState);
            ReattachHelperButtons();

            if (clearCache) {
                parser.ClearCache();
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

        private void InitPolygons() {
            parser.CreatePolygonsFromConfiguration();
            DrawProjection();
        }

        private void DrawProjection(bool resetPolygons = true) {

            //get the projection type from the gui radio buttons
            var type = GetProjectionType();


            if (resetPolygons) {
                //clear canvas
                Clear();
                //reload original positions
                parser.CreatePolygonsFromConfiguration();
                ScaleShapes(totalScaledValue, false);
            }

            FillPolygons();
            List<MyPolygon> projectedPolygons = new List<MyPolygon>();

            for (int i = 0; i < parser.polygonList.Count; i++) {
                projectedPolygons.Add(parser.polygonList[i].PerformProjection(GetProjectionType()));
            }
            projectedPolygons.Sort(); //deep sort by Z axis
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

        private void DrawPolygons(List<MyPolygon> polygonList) {
            polygonList.Sort();
            foreach (var polygon in polygonList) {
                var newPoly = new Polygon {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Points = new PointCollection()
                };

                //create new point collection with calculated offset before drawing on canvas:
                for(int i=0; i < polygon.poly.Points.Count ; i++) {
                    newPoly.Fill = polygon.poly.Fill;
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
                projectedPolygons.Sort(); //deep sort by Z axis
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



            var axis = Axis.X;
            var value = Double.Parse(TransitionValueTb.Text);

            foreach (RadioButton radioBtn in TransitionGrpPanel.Children) {
                if (radioBtn.IsChecked == true) {
                    axis = GetAxis(radioBtn);
                    break;
                }
            }

            if (!IsValidTransitionValue(axis, value)) {
                return;
            }
            else {
                //clear canvas
                Clear(false);
            }

            List<MyPolygon> projectedPolygons = new List<MyPolygon>();
            for (int i = 0; i < parser.polygonList.Count; i++) {
                parser.polygonList[i].Transition(axis, value);
                projectedPolygons.Add(parser.polygonList[i].PerformProjection(GetProjectionType()));
            }
            DrawPolygons(projectedPolygons);

        }

        private bool IsValidTransitionValue(Axis axis, double value) {
            double min = 0.0;
            double max = 0.0;
            GetAxisMinMaxValues(axis, ref min, ref max);
            foreach (var poly in parser.polygonList) {
                if (!poly.PredictTransitionValidity(axis, value, min, max)) {
                    DisplayIllegalValueNotificiation("Operation failed! transition was too high and went out of screen borders.", true);
                    return false;
                }
            }
            return true;
        }

        private void GetAxisMinMaxValues(Axis axis, ref double min, ref double max) {
            var toolBarOffset = 50.0;
            var invisOffset = 50.0;

            switch (axis) {
                case Axis.X:
                    min = -((Width) / 2) +invisOffset;     //max x on screen before offset addition
                    max = min * (-1);       //max x on screen before offset addition
                    break;
                case Axis.Y:
                    min = -(Height / 2) + toolBarOffset;    //min y on screen before offset addition
                    max = min * (-1);                       //max y on screen before offset addition
                    break;
                case Axis.Z:
                    min = Transformations.zDistanceMinMax.X;    //min z 
                    max = Transformations.zDistanceMinMax.Y;    //max z
                    break;
            }

        }

        public void OnBtnApplyScalingClicked(object sender, RoutedEventArgs e) {
            DisplayIllegalValueNotificiation("", false);
            var scaleValue = Double.Parse(ScalingValueTb.Text);
            ScaleShapes(scaleValue);

        }

        private void ScaleShapes(double scaleValue, bool updateTotalScaledValue = true) {
            //clear canvas
            Clear(false);

            if (updateTotalScaledValue) {
                UpdateTotalScaledValue(scaleValue);
            }

            List<MyPolygon> projectedPolygons = new List<MyPolygon>();

            for (int i = 0; i < parser.polygonList.Count; i++) {
                parser.polygonList[i].Scale(scaleValue);
                projectedPolygons.Add(parser.polygonList[i].PerformProjection(GetProjectionType()));
            }

            if (ShapesOutOfBordersAfterScaling()) {
                ScaleShapesBackToRecentSize();
            }

            DrawPolygons(projectedPolygons);
        }

        //save total scaled value to keep correct size when swapping views
        private void UpdateTotalScaledValue(double scaleValue) {
            totalScaledValue *= scaleValue;
            Console.WriteLine("totalScaledValue : " + totalScaledValue);
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
            DisplayIllegalValueNotificiation();
            RotationValueTb.Text = slider.Value.ToString();
        }


        //validate legal angle value on input entered
        private void OnTransitionValueInputTextChanged(object sender, TextChangedEventArgs args) {
            var MAX_VALUE_DIGITS = 4;
            var textBox = sender as TextBox;
            var illegalInputMsg = "Input should only contain \nnumbers between " + Width * (-1) + " and " + Width + "!";

            Regex negRgx = new Regex(@"^-[0-9]+$");
            Regex posRgx = new Regex(@"^[0-9]+$");

            Match negMatch = negRgx.Match(TransitionValueTb.Text);
            Match posMatch = posRgx.Match(TransitionValueTb.Text);

            DisplayIllegalValueNotificiation(null, false);

            if (!posMatch.Success && !negMatch.Success) {
                Console.WriteLine("OnTransitionValueInputTextChanged.");
                DisplayIllegalValueNotificiation(illegalInputMsg, true);
                if (TransitionValueTb.Text.Length > 0 && !TransitionValueTb.Text.Contains("-")) {
                    TransitionValueTb.Text = TransitionValueTb.Text.Substring(0, TransitionValueTb.Text.Length - 1);
                }
            }

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
                    textBox.Text = "";
                    DisplayIllegalValueNotificiation(illegalInputMsg);
                }
            }
            return;
        }

        //notification msg on illegal operation from user, such as wrong input:
        private void DisplayIllegalValueNotificiation(string msg = "", bool display = true) {
            if (UserNotificationBorder != null && UserNotification != null) {
                UserNotificationBorder.Visibility = display ? Visibility.Visible : Visibility.Hidden;
                UserNotification.Text = display ? msg : "";
            }
        }

        //angle value range validity
        private bool IsInValidRange(double angleValue) {
            return angleValue >= 0 && angleValue <= 360;
        }

        //display or hide help windows on toggle
        public void OnBtnHelpClicked(object sender, RoutedEventArgs e) {
            DisplayIllegalValueNotificiation("",false);
            helpWindow.Visibility = helpWindow.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }

        private void OnProjectionChanged(object sender, RoutedEventArgs e) {
            if(myCanvas != null) {
                DrawProjection();
            }
        }

        private void OnInvisSurfaceBtnClicked(object sender, RoutedEventArgs e) {

            if(showHiddenSurfaceCb.Content.ToString().Contains("Show")) {
                Console.WriteLine("OnDeepSurfaceBtnClicked with Show");

                showHiddenSurfaceCb.Content = "Hide invisible surfaces";
                showInvisibleSurface = true;
            }
            else {
                showHiddenSurfaceCb.Content = "Show invisible surfaces";
                Console.WriteLine("OnDeepSurfaceBtnClicked with Hide");
                showInvisibleSurface = false;
            }
            DrawProjection(false);
        }

        //fill polygons with solid or transperant color according to configuration:
        private void FillPolygons(bool? forceFill = null) {
            var transperentPoly = new Polygon();

            if(!(forceFill == null)) {
                showInvisibleSurface = forceFill.Value;
            }

            Console.WriteLine("FillPolygons : " + showInvisibleSurface);
            foreach (var myPoly in parser.polygonList) {
                if (showInvisibleSurface) {
                    myPoly.poly.Fill = transperentPoly.Fill;
                }
                else {
                    myPoly.poly.Fill = Brushes.AliceBlue;
                }
            }
        }



    }
}
