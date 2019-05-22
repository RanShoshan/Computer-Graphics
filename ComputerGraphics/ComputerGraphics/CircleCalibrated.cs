/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Windows;

namespace ComputerGraphics {
    internal class CircleCalibrated {
        internal Circle uncalibrated;
        internal Circle calibrated;

        public CircleCalibrated(Circle circle) {
            uncalibrated = circle;
            calibrated = CalculateCalibratedCircle(uncalibrated);
        }

        private Circle CalculateCalibratedCircle(Circle uncalibrated) {
            Point p1 = new Point();
            Point p2 = new Point();

            p1.Y = 0.0;
            p1.X = 0.0;
            p2.Y = uncalibrated.pt2.Y - uncalibrated.pt1.Y;
            p2.X = uncalibrated.pt2.X - uncalibrated.pt1.X;

            return new Circle(p1, p2);
        }

        internal void FixCalibrationOffset() {
            calibrated.pt1.Y += uncalibrated.pt1.Y;
            calibrated.pt1.X += uncalibrated.pt1.X;
            calibrated.pt2.Y += uncalibrated.pt1.Y;
            calibrated.pt2.X += uncalibrated.pt1.X;
        }
    }
}