using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BotLib.Wpf.Extensions
{
    public static class FrameworkElementEx
    {
        public static void xInvoke(this FrameworkElement elem, Action act)
        {
            elem.Dispatcher.Invoke(act, new object[0]);
        }

        public static void xSetWidth(this FrameworkElement elem, double width)
        {
            elem.MinWidth = width;
            elem.MaxWidth = width;
            if (elem is TextBlock)
            {
                TextBlock textBlock = elem as TextBlock;
                textBlock.TextWrapping = TextWrapping.Wrap;
            }
        }

        public static void xSetHeight(this FrameworkElement elem, double height)
        {
            elem.MinHeight = height;
            elem.MaxHeight = height;
        }
    }
}
