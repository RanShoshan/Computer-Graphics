using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace ComputerGraphics {
    public enum ShapeName {
        CIRCLE,
        LINE,
        BEZIER,
        NONE
    }

    class FileParserUtil {
        
        public readonly List<Bezier> bezierList = new List<Bezier>();
        public readonly List<Circle> circleList = new List<Circle>();
        public readonly List<Line> lineList = new List<Line>();
        private readonly char delim = ',';

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
                AddShapeToList(shapeName, lines[i].Split(delim));
            }
        }

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
                default:
                    break;
            }
        }

        private void AddBezierToList(string[] vals) {
            Bezier b = new Bezier(
                vals[0], vals[1], vals[2], vals[3],
                vals[4], vals[5], vals[6], vals[7]);
        }

        private void AddCircleToList(string[] vals) {
            circleList.Add(new Circle(vals[0], vals[1], vals[2], vals[3]));
        }

        private void AddLineToList(string[] vals) {
            lineList.Add(new Line(vals[0], vals[1], vals[2], vals[3]));
        }

        private ShapeName GetShapeName(string v) {
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

        internal void ClearCache() {
            lineList.Clear();
            circleList.Clear();
            bezierList.Clear();
        }
    }
}
