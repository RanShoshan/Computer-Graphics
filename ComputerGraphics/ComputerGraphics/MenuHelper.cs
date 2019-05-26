using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphics {
    public static class MenuHelper {
        public static string MENU_TEXT = 
            "Buttons from top to bottom:\n\n\n" +
            "Clear - clears entire screen\n\n" +
            "Save - saves the current canvas state to the LOADED file\n\n" +
            "Load - loads an existing file\n\n" +
            "Line - select 2 points to draw a line between the two\n\n" +
            "Circle - select first point to set a center, then second point for a radius from center\n\n" +
            "Bezier Curve - set number of lines in the below text box and place 4 control points\n\n" +
            "Move - drag the orange anchor point to trasnform shapes over the canvas\n\n" +
            "Scale Up - click to scale up all shapes\n\n" +
            "Scale Down - click to scale down all shapes\n\n" +
            "Shear on X axis - drag the orange anchor point (left/right) to shear to that point\n\n" +
            "Shear on Y axis - drag the orange anchor point (up/down) to shear to that point\n\n" +
            "Mirror on Y axis - click to mirror shapes over Y axis\n\n" +
            "Mirror on X axis - click to mirror shapes over X axis\n\n" +
            "Rotate - set the angle (-360 to 360) and drag the orange anchor point to rotate shapes\n\n" +
            "\n*(You can also hover over the icons to see detailed description)\n";
    }
}
