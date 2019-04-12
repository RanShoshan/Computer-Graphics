/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Windows;
using System.Windows.Controls;

namespace ComputerGraphics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();


            
            
            
        }

        public void OnBtnLineClicked(object sender, RoutedEventArgs e) {
            for (int i = 0; i < 500; i++) {
                SetPixel(100 + i, 100 + i);
            }
        }

        private void SetPixel(int x, int y) {

            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Stroke = System.Windows.Media.Brushes.Blue;
            rect.StrokeThickness = 1;
            rect.Width = 1;
            rect.Height = 1;
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            myCanvas.Children.Add(rect);
        }

        
    }



}
