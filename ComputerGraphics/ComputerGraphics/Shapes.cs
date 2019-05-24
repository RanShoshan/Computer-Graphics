using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerGraphics {
    class Shapes {
    }

    public class Bezier {
        public static readonly string DEFAULT_SMOOTHING_RATE = "30";
        public Point cp1, cp2, cp3, cp4;

        public Bezier() { }
        public Bezier(string x1, string y1, string x2, string y2,
            string x3, string y3, string x4, string y4) {
            cp1 = new Point(Double.Parse(x1), Double.Parse(y1));
            cp2 = new Point(Double.Parse(x2), Double.Parse(y2));
            cp3 = new Point(Double.Parse(x3), Double.Parse(y3));
            cp4 = new Point(Double.Parse(x4), Double.Parse(y4));

        }

        internal void Move(double dx, double dy) {
            cp1.X += dx;
            cp2.X += dx;
            cp3.X += dx;
            cp4.X += dx;

            cp1.Y += dy;
            cp2.Y += dy;
            cp3.Y += dy;
            cp4.Y += dy;
        }
    }

    public class Circle : MyLine {
        public Circle(string centerX, string centerY, string endX, string endY) 
            : base(centerX, centerY, endX, endY) {}

        public Circle(Point p1, Point p2)
            : base(p1, p2) { }
    }

    public class MyLine {
        public Point pt1;
        public Point pt2;

        public MyLine() {
        }

        public MyLine(Point p1, Point p2) {
            pt1 = p1;
            pt2 = p2;
        }

        public MyLine(string x1, string y1, string x2, string y2) {
            pt1 = new Point(Double.Parse(x1), Double.Parse(y1));
            pt2 = new Point(Double.Parse(x2), Double.Parse(y2));
        }

        internal void Move(double dx, double dy) {
            pt1.X += dx;
            pt1.Y += dy;
            pt2.X += dx;
            pt2.Y += dy;
        }

        internal void Rotate() {
            //var lineCenterPointX = pt1.X - pt2.X;
            //var lineCenterPointY = pt1.Y - pt2.Y;
            //Point lineCenterPoint = new Point(lineCenterPointX, lineCenterPointY);

            //RotatePoint(pt1, lineCenterPoint, 20);
        }
    }

}
