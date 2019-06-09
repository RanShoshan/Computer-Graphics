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
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Media;

namespace ComputerGraphics {
    public enum ShapeName {
        CIRCLE,
        LINE,
        BEZIER,
        POLYGON,
        VERTEX,
        NONE
    }

    //Parser util - reads shape values from configuration file and creates matching objects
    class FileParserUtil {
        
        public readonly List<Bezier> bezierList = new List<Bezier>();
        public readonly List<Circle> circleList = new List<Circle>();
        public readonly List<MyLine> lineList = new List<MyLine>();
        public readonly List<Point3D> vertexList = new List<Point3D>();
        public readonly List<MyPolygon> polygonList = new List<MyPolygon>();

        public static readonly char delimiter = ',';
    
        //Parse the file according to our format
        public void ParseFile(string fileName) {

            var shapeName = ShapeName.NONE;
            string[] full_file = File.ReadAllLines(fileName);
            List<string> lines = new List<string>();
            lines.AddRange(full_file);
            for (int i = 0; i < lines.Count; i++) {
                if(GetShapeName(lines[i]) != ShapeName.NONE) {
                    shapeName = GetShapeName(lines[i]);
                    continue;
                }
                AddShapeToList(shapeName, lines[i].Split(delimiter));
            }
        }

        //Create and add new objects according to their matching shape type
        private void AddShapeToList(ShapeName shapeName, string[] vals) {
            switch (shapeName) {
                case ShapeName.LINE:
                    AddLineToList(vals);
                    break;
                case ShapeName.CIRCLE:
                    AddCircleToList(vals);
                    break;
                case ShapeName.BEZIER:
                    AddBezierToList(vals);
                    break;
                case ShapeName.VERTEX:
                    AddVertexToList(vals);
                    break;
                case ShapeName.POLYGON:
                    AddPolygonToList(vals);
                    break;
                default:
                    break;
            }
        }

        private void AddPolygonToList(string[] vals) {
            int[] vertexIndexes = Array.ConvertAll(vals, int.Parse);
            List<Point3D> tempVertexList = new List<Point3D>();

            foreach (var idx in vertexIndexes) {
                tempVertexList.Add(vertexList[idx]);
            }

            var poly = new Polygon { Stroke = Brushes.Black, StrokeThickness = 1, Fill = Brushes.White };
            poly.Points = BuildPointCollection(tempVertexList);
            polygonList.Add(new MyPolygon(poly, tempVertexList));
        }

        private PointCollection BuildPointCollection(List<Point3D> tempVertexList) {
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

        private void AddVertexToList(string[] vals) {
            vertexList.Add(new Point3D(
                Double.Parse(vals[0]), Double.Parse(vals[1]), Double.Parse(vals[2])));
        }

        private void AddBezierToList(string[] vals) {
            bezierList.Add(new Bezier(
                vals[0], vals[1], vals[2], vals[3],
                vals[4], vals[5], vals[6], vals[7]));
        }

        private void AddCircleToList(string[] vals) {
            circleList.Add(new Circle(vals[0], vals[1], vals[2], vals[3]));
        }

        private void AddLineToList(string[] vals) {
            lineList.Add(new MyLine(vals[0], vals[1], vals[2], vals[3]));
        }

        private ShapeName GetShapeName(string v) {
            if (v.Contains(ShapeName.VERTEX.ToString())) {
                return ShapeName.VERTEX;
            }
            if (v.Contains(ShapeName.POLYGON.ToString())) {
                return ShapeName.POLYGON;
            }
            if (v.Contains(ShapeName.LINE.ToString())) {
                return ShapeName.LINE;
            }
            if (v.Contains(ShapeName.CIRCLE.ToString())) {
                return ShapeName.CIRCLE;
            }
            if (v.Contains(ShapeName.BEZIER.ToString())) {
                return ShapeName.BEZIER;
            }
            return ShapeName.NONE;
        }

        //Clear lists
        internal void ClearCache() {
            lineList.Clear();
            circleList.Clear();
            bezierList.Clear();
        }
    }
}
