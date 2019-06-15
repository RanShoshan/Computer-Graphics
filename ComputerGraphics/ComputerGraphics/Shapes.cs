/*
 * ran shoshan 308281575
 * &
 * shay rubach 305687352
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ComputerGraphics {
    class Shapes {
    }

    public static class ShapesHelper {
        internal static void MultPointBy(ref Point p, double val) {
            p.X *= val;
            p.Y *= val;
        }

        //Helper functions to get min and max X or Y from all shapes:
        internal static double GetMinX(MyLine line) {
            return Math.Min(line.pt2.X, line.pt1.X);
        }

        internal static double GetMinY(MyLine line) {
            return Math.Min(line.pt2.Y, line.pt1.Y);
        }

        internal static double GetMaxY(MyLine line) {
            return Math.Max(line.pt2.X, line.pt1.X);
        }

        internal static double GetMaxX(MyLine line) {
            return Math.Max(line.pt2.Y, line.pt1.Y);
        }

        internal static double GetMinX(Bezier b) {
            var val1 = Math.Min(b.cp1.X, b.cp2.X);
            var val2 = Math.Min(b.cp3.X, b.cp4.X);
            return Math.Min(val1, val2);
        }

        internal static double GetMinY(Bezier b) {
            var val1 = Math.Min(b.cp1.Y, b.cp2.Y);
            var val2 = Math.Min(b.cp3.Y, b.cp4.Y);
            return Math.Min(val1, val2);
        }

        internal static double GetMaxY(Bezier b) {
            var val1 = Math.Max(b.cp1.Y, b.cp2.Y);
            var val2 = Math.Max(b.cp3.Y, b.cp4.Y);
            return Math.Min(val1, val2);
        }

        internal static double GetMaxX(Bezier b) {
            var val1 = Math.Max(b.cp1.X, b.cp2.X);
            var val2 = Math.Max(b.cp3.X, b.cp4.X);
            return Math.Min(val1, val2);
        }

        internal static PointCollection BuildPointCollection(List<Point3D> tempVertexList) {
            var pc = new PointCollection();
            //triangle (pyramid):
            pc.Add(new Point(tempVertexList[0].X, tempVertexList[0].Y));
            pc.Add(new Point(tempVertexList[1].X, tempVertexList[1].Y));
            pc.Add(new Point(tempVertexList[2].X, tempVertexList[2].Y));
            //square (cube):
            if (tempVertexList.Count == 4) {
                pc.Add(new Point(tempVertexList[3].X, tempVertexList[3].Y));
            }
            return pc;
        }
    }

    //Bezier Curve representation
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

        //Simple Scaling algorithm
        internal void Scale(double val) {
            ShapesHelper.MultPointBy(ref cp1, val);
            ShapesHelper.MultPointBy(ref cp2, val);
            ShapesHelper.MultPointBy(ref cp3, val);
            ShapesHelper.MultPointBy(ref cp4, val);
        }

        //Transition algorithm
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

        //Mirroring algorithm accordign to direction (x or y)
        internal void Mirror(Point centerPoint, MirrorDirection direction) {
            switch (direction) {
                case MirrorDirection.Y:
                    cp1.X = centerPoint.X + (centerPoint.X - cp1.X);
                    cp2.X = centerPoint.X + (centerPoint.X - cp2.X);
                    cp3.X = centerPoint.X + (centerPoint.X - cp3.X);
                    cp4.X = centerPoint.X + (centerPoint.X - cp4.X);
                    break;
                case MirrorDirection.X:
                    cp1.Y = centerPoint.Y + (centerPoint.Y - cp1.Y);
                    cp2.Y = centerPoint.Y + (centerPoint.Y - cp2.Y);
                    cp3.Y = centerPoint.Y + (centerPoint.Y - cp3.Y);
                    cp4.Y = centerPoint.Y + (centerPoint.Y - cp4.Y);
                    break;
            }

        }

    }

    public class MyPolygon {
        public Polygon poly;
        public List<Point3D> vertexes = new List<Point3D>();
        public int[] vertexIndexes;

        public MyPolygon(Polygon poly, List<Point3D> vertexes, int[] vertexIndexes) {
            this.poly = poly;
            this.vertexes = vertexes;
            this.vertexIndexes = vertexIndexes;
        }

        internal void Scale(double scaleValue) {

            var newVertextes = new List<Point3D>();

            foreach (var vertex in vertexes) {
                newVertextes.Add(Transformations.Scale(vertex, scaleValue));
            }

            vertexes = newVertextes;
            poly.Points = ShapesHelper.BuildPointCollection(newVertextes);

        }
    }

    //Circle representation
    public class Circle : MyLine {
        public Circle(string centerX, string centerY, string endX, string endY) 
            : base(centerX, centerY, endX, endY) {}

        public Circle(Point p1, Point p2)
            : base(p1, p2) { }
    }

    //Line representation
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

        //Transition algorithm
        internal void Move(double dx, double dy) {
            pt1.X += dx;
            pt1.Y += dy;
            pt2.X += dx;
            pt2.Y += dy;
        }

        //Simple Scaling algorithm
        internal void Scale(double val) {
            ShapesHelper.MultPointBy(ref pt1, val);
            ShapesHelper.MultPointBy(ref pt2, val);
        }
        
        //Mirroring algorithm accordign to direction (x or y)
        internal void Mirror(Point centerPoint, MirrorDirection direction) {
            switch(direction) {
                case MirrorDirection.Y:
                    pt1.X = centerPoint.X + (centerPoint.X - pt1.X);
                    pt2.X = centerPoint.X + (centerPoint.X - pt2.X);
                    break;
                case MirrorDirection.X:
                    pt1.Y = centerPoint.Y + (centerPoint.Y - pt1.Y);
                    pt2.Y = centerPoint.Y + (centerPoint.Y - pt2.Y);
                    break;
            }
        }
    }

}
