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
            poly.Points = ShapesHelper.BuildPointCollection(tempVertexList);
            polygonList.Add(new MyPolygon(poly, tempVertexList, vertexIndexes));
        }

        private void AddVertexToList(string[] vals) {
            vertexList.Add(new Point3D(
                Double.Parse(vals[0]), Double.Parse(vals[1]), Double.Parse(vals[2])));
        }

        private ShapeName GetShapeName(string v) {
            if (v.Contains(ShapeName.VERTEX.ToString())) {
                return ShapeName.VERTEX;
            }
            if (v.Contains(ShapeName.POLYGON.ToString())) {
                return ShapeName.POLYGON;
            }
            return ShapeName.NONE;
        }

        //Clear lists
        internal void ClearCache() {
            polygonList.Clear();
        }

    }
}
