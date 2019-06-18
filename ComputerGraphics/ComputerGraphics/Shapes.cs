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

    public static class ShapesHelper {

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

        internal void Rotate(Axis axis, double angle) {
            var newVertextes = new List<Point3D>();
            foreach (var vertex in vertexes) {
                newVertextes.Add(Transformations.Rotate(vertex, axis, angle));
            }
            vertexes = newVertextes;
            poly.Points = ShapesHelper.BuildPointCollection(newVertextes);
        }

        internal void Transition(Axis axis, double value) {
            var newVertextes = new List<Point3D>();
            foreach (var vertex in vertexes) {
                newVertextes.Add(Transformations.Transition(vertex, axis, value));
            }
            vertexes = newVertextes;
            poly.Points = ShapesHelper.BuildPointCollection(newVertextes);
        }

        internal MyPolygon PerformProjection(ProjectionType type) {
            var newVertextes = new List<Point3D>();
            
            foreach (var vertex in vertexes) {
                switch (type) {
                    default:
                    case ProjectionType.ORTHOGRAPHIC:
                        newVertextes.Add(Transformations.Orthographic(vertex));
                        break;
                    case ProjectionType.OBLIQUE:
                        newVertextes.Add(Transformations.Oblique(vertex));
                        break;
                    case ProjectionType.PERSPECTIVE:
                        newVertextes.Add(Transformations.Perspective(vertex));
                        break;
                }
            }

            var newVertexIndexes = vertexIndexes;
            var newPoly = new Polygon { Stroke = Brushes.Black, StrokeThickness = 1};
            newPoly.Points = ShapesHelper.BuildPointCollection(newVertextes);

            return new MyPolygon(newPoly, newVertextes, newVertexIndexes);
        }

        internal bool PredictTransitionValidity(Axis axis, double transValue, double min, double max) {
            foreach (var pt in poly.Points) {
                var comparableValue = 0.0;
                switch (axis) {
                    case Axis.X: comparableValue = pt.X; break;
                    case Axis.Y: comparableValue = pt.Y; break;
                }

                if(comparableValue + transValue > max || comparableValue - transValue < min) {
                    return false;
                }
            }

            return true;
        }
    }

}
