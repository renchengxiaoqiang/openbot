using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotLib.Wpf.Extensions
{
    public static class PointEx
    {
        public static Point xAdd(this Point p, double dx, double dy)
        {
            return new Point(p.X + dx, p.Y + dy);
        }

        public static Point xConvertLogicalToPhisical(this Point p)
        {
            return new Point(p.X / VisualEx.XRatioOfLogicalVsPhysical, p.Y / VisualEx.YRatioOfLogicalVsPhysical);
        }

        public static Point xConvertPhisicalToLogical(this Point p)
        {
            return new Point(p.X * VisualEx.XRatioOfLogicalVsPhysical, p.Y * VisualEx.YRatioOfLogicalVsPhysical);
        }
    }
}
