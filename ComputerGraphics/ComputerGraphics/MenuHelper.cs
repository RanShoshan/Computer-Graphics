/*
 * ran shoshan 308281575
 * shay rubach 305687352
 * yaniv yona 203455266
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphics {
    public static class MenuHelper {
        public static string MENU_TEXT = 
            "\nGuide:\n" +
            "You can choose between 3 optional views:\n" +
            "\t - Orthographic view\n" +
            "\t - Oblique (Cabinet) view\n" +
            "\t - Perspective view\n\n" +
            "Available transformations:\n" +
            "\t - Rotation: choose axis, insert an angle between -360 and 360, press apply\n" +
            "\t - Transition: choose axis, insert a value, press apply\n" +
            "\t - Scaling: insert scaling factor and press apply\n" +
            "\n\n" +
            "Show/Hide invisible surfaces button:\n" +
            "Hide or show the 'insivible' surfaces - does not work well at the moment\n\n\n" +
            "(c) Shay Rubach, Ran Shoshan, Yaniv Yona";
    }
}
