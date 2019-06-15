using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ComputerGraphics {
    static class Transformations {

        //scale polygon
        public static Point3D Scale(Point3D pt, double scaleValue) {
            return new Point3D(pt.X * scaleValue, pt.Y * scaleValue, pt.Z * scaleValue);
        }
    }
}
