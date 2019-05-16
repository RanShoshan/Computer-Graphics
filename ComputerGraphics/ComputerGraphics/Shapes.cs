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
    }

    public class Circle : Line {
        public Circle(string centerX, string centerY, string endX, string endY) 
            : base(centerX, centerY, endX, endY) {}
    }

    public class Line {
        public Point pt1;
        public Point pt2;

        public Line(Point p1, Point p2) {
            pt1 = p1;
            pt2 = p2;
        }

        public Line(string x1, string y1, string x2, string y2) {
            pt1 = new Point(Double.Parse(x1), Double.Parse(y1));
            pt2 = new Point(Double.Parse(x2), Double.Parse(y2));
        }


    }


}
