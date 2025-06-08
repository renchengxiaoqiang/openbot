using BotLib.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BotLib
{
    public static class WpfUtil
    {
        public static GridLength ConvertPixelWidthToGridLength(int w)
        {
            return new GridLength(WpfUtil.ConvertPixelWidthToLogical(w));
        }

        public static GridLength ConvertPixelHeightToGridLength(int h)
        {
            return new GridLength(WpfUtil.ConvertPixelHeightToLogical(h));
        }

        public static double ConvertPixelWidthToLogical(int w)
        {
            return VisualEx.XRatioOfLogicalVsPhysical * (double)w;
        }

        public static double ConvertPixelHeightToLogical(int h)
        {
            return VisualEx.YRatioOfLogicalVsPhysical * (double)h;
        }
    }
}
