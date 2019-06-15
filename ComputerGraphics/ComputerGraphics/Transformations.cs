using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace ComputerGraphics {
    static class Transformations {

        public static readonly Point3D shapesMiddlePt = new Point3D(100.0, -100.0, -50.0);
        public static readonly Point3D userPov = new Point3D(0.0, 0.0, -500.0);

        //min and max values for Z axis distance (artificial) on screen:
        public static readonly Point zDistance = new Point(-500.0, 500.0);


        public static Point screen;

        //scale polygon
        public static Point3D Scale(Point3D pt, double scaleValue) {
            return new Point3D(pt.X * scaleValue, pt.Y * scaleValue, pt.Z * scaleValue);
        }

        internal static Point3D Rotate(Point3D vertex, Axis axis, double angle) {

            //calculate offset values
            var xCenter = Math.Abs(screen.X / 2);
            var yCenter = Math.Abs(screen.Y / 2);
            var zCenter = Math.Abs(zDistance.X - zDistance.Y) / 2;

            //our distance vector
            double[] distanceVector = { xCenter, yCenter, zCenter };

            //base matrix representation of our vertex:
            double[,] baseVertexMtx = { { vertex.X, vertex.Y, vertex.Z, 1 } };

            //post rotation processed matrix
            double[,] rotatedMtx;

            switch (axis) {
                default:
                case Axis.X:
                    rotatedMtx = MtxHelper.MtxMultiply(baseVertexMtx, RotateXMatrix(angle));
                    break;
                case Axis.Y:
                    rotatedMtx = MtxHelper.MtxMultiply(baseVertexMtx, RotateYMatrix(angle));
                    break;
                case Axis.Z:
                    rotatedMtx = MtxHelper.MtxMultiply(baseVertexMtx, RotateZMatrix(angle));
                    break;
            }
            
            //return new rotated 3d point:
            return new Point3D(rotatedMtx[0,0], rotatedMtx[0,1], rotatedMtx[0,2]);
        }

        //matrix builder functions:
        //matrix for X rotation
        private static double[,] RotateXMatrix(double angle) {
            //refactor this sentence and use in other places:
            angle = (angle * (Math.PI / 180));
            double[,] mtx = {
                { 1, 0, 0, 0 },
                { 0, Math.Cos(angle), Math.Sin(angle), 0 },
                { 0, -Math.Sin(angle), Math.Cos(angle), 0 },
                { 0, 0, 0, 1 }
            };
            return mtx;
        }

        //matrix for Y rotation
        private static double[,] RotateYMatrix(double angle) {
            angle = (angle * (Math.PI / 180));
            double[,] mtx = {
                { Math.Cos(angle), 0, -Math.Sin(angle), 0 },
                { 0, 1, 0, 0 },
                { Math.Sin(angle), 0, Math.Cos(angle), 0 },
                { 0, 0, 0, 1 }
            };
            return mtx;
        }

        //matrix for Z rotation
        private static double[,] RotateZMatrix(double angle) {
            angle = (angle * (Math.PI / 180));
            double[,] mtx = {
                { Math.Cos(angle), Math.Sin(angle), 0, 0 },
                { -Math.Sin(angle), Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            };
            return mtx;
        }


        //projections implementation:
        //orthographic:
        public static Point3D Orthographic(Point3D vertex, double angle = 0.0) {

            //cos and sin angle calculation
            var cos = (Math.Cos(angle * (Math.PI / 180)));
            var sin = (Math.Sin(angle * (Math.PI / 180)));

            //base matrix representation of our vertex:
            double[,] baseVertexMtx = { { vertex.X, vertex.Y, vertex.Z, 1 } };

            //orthographic matrix representation
            double[,] orthographicMtx = { 
                { 1, 0, 0, 0 }, 
                { 0, 1, 0, 0 }, 
                { cos, sin, 0, 0 }, 
                { 0, 0, 0, 1 }
            };

            //resulted processed matrix
            double[,] processedMtx = MtxHelper.MtxMultiply(baseVertexMtx, orthographicMtx);

            return new Point3D(processedMtx[0, 0], processedMtx[0, 1], processedMtx[0, 2]);
        }


        //oblique:
        public static Point3D Oblique(Point3D vertex) {
            //base matrix representation of our vertex:
            double[,] baseVertexMtx = { { vertex.X, vertex.Y, vertex.Z, 1 } };
            
            //oblique matrix representation
            double[,] obliqueMtx = { 
                { 1, 0, 0, 0 }, 
                { 0, 1, 0, 0 }, 
                { 0, 0, 0, 0 }, 
                { 0, 0, 0, 1 }
            };
            
            //resulted processed matrix
            double[,] processedMtx = MtxHelper.MtxMultiply(baseVertexMtx, obliqueMtx);
            return new Point3D(processedMtx[0, 0], processedMtx[0, 1], processedMtx[0,2]);
        }



        //perspective:
        public static Point3D Perspective(Point3D vertex) {
            //base matrix representation of our vertex:
            double[,] baseVertexMtx = { { vertex.X, vertex.Y, vertex.Z, 1 } };

            double Z;

            Z = 1 / (1 + (vertex.Z / userPov.Z));

            //perspective matrix representation
            double[,] perspectiveMtx = { 
                { Z, 0, 0, 0 }, 
                { 0, Z, 0, 0 }, 
                { 0, 0, 0, 0 }, 
                { 0, 0, 0, 1 }
            };
            
            //resulted processed matrix
            double[,] processedMtx = MtxHelper.MtxMultiply(baseVertexMtx, perspectiveMtx);
            return new Point3D(processedMtx[0, 0], processedMtx[0, 1], processedMtx[0, 2]);
        }

    }

    


    //used fot matrix to matrix calculation etc:
    internal static class MtxHelper {

        public static double[,] MtxMultiply(double[,] A, double[,] B) {
            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);
            double temp = 0;
            double[,] kHasil = new double[rA, cB];
            if (cA != rB) {
                Console.WriteLine("matrix can't be multiplied!");
                return null;
            }
            else {
                for (int i = 0; i < rA; i++) {
                    for (int j = 0; j < cB; j++) {
                        temp = 0;
                        for (int k = 0; k < cA; k++) {
                            temp += A[i, k] * B[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
                return kHasil;
            }
        }
    }
}
