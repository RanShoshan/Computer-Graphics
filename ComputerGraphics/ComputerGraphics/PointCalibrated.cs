/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Windows;

namespace ComputerGraphics {
    internal class LineCalibrated {
        internal Line uncalibrated;
        internal Line calibrated;

        internal LineCalibrated(Line line) {
            uncalibrated = line;
            calibrated = CalculateCalibratedPoint(uncalibrated);
        }

        private Line CalculateCalibratedPoint(Line uncalibrated) {
            Point p1 = new Point();
            Point p2 = new Point();

            if (uncalibrated.pt1.X < uncalibrated.pt2.X) {
                p1.Y = 0.0;
                p1.X = 0.0;
                p2.Y = uncalibrated.pt2.Y - uncalibrated.pt1.Y;
                p2.X = uncalibrated.pt2.X - uncalibrated.pt1.X;
            }
            else {
                p2.Y = 0.0;
                p2.X = 0.0;
                p1.Y = uncalibrated.pt1.Y - uncalibrated.pt2.Y;
                p1.X = uncalibrated.pt1.X - uncalibrated.pt2.X;
            }

            return new Line(p1, p2);
        }

        internal void FixCalibrationOffset() {
            if (uncalibrated.pt1.X < uncalibrated.pt2.X) {
                calibrated.pt1.Y += uncalibrated.pt1.Y;
                calibrated.pt1.X += uncalibrated.pt1.X;
                calibrated.pt2.Y += uncalibrated.pt1.Y;
                calibrated.pt2.X += uncalibrated.pt1.X;
            }
            else {
                calibrated.pt1.Y += uncalibrated.pt2.Y;
                calibrated.pt1.X += uncalibrated.pt2.X;
                calibrated.pt2.Y += uncalibrated.pt2.Y;
                calibrated.pt2.X += uncalibrated.pt2.X;
            }
        }
    }
}