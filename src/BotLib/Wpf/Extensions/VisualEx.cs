using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace BotLib.Wpf.Extensions
{
    public static class VisualEx
    {
        public static Point PointToScreenLogical(this Visual visual, Point p)
        {
            Point point = visual.PointToScreen(p);
            return new Point
            {
                X = point.X * XRatioOfLogicalVsPhysical,
                Y = point.Y * YRatioOfLogicalVsPhysical
            };
        }

        public static Point LocationInContainer(this Visual v, Visual ancestor)
        {
            return v.TransformToAncestor(ancestor).Transform(new Point(0.0, 0.0));
        }

        public static double XRatioOfLogicalVsPhysical
        {
            get
            {
                if (_xRatioOfLogicalVsPhysical == 0.0)
                {
                    _xRatioOfLogicalVsPhysical = SystemParameters.WorkArea.Width / (double)Screen.PrimaryScreen.WorkingArea.Width;
                }
                return _xRatioOfLogicalVsPhysical;
            }
        }

        public static double YRatioOfLogicalVsPhysical
        {
            get
            {
                if (_yRatioOfLogicalVsPhysical == 0.0)
                {
                    _yRatioOfLogicalVsPhysical = SystemParameters.WorkArea.Height / (double)Screen.PrimaryScreen.WorkingArea.Height;
                }
                return _yRatioOfLogicalVsPhysical;
            }
        }

        private static double _xRatioOfLogicalVsPhysical = 0.0;

        private static double _yRatioOfLogicalVsPhysical = 0.0;
    }
}
