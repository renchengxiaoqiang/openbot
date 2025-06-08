using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BotLib.Wpf.Extensions
{
    public static class UIElementEx
    {
        public static void xRaiseMouseDownEvent(this UIElement elem, MouseButtonEventArgs e = null)
        {
            if (e == null)
            {
                elem.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent,
                    Source = elem
                });
            }
            else
            {
                elem.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
                {
                    RoutedEvent = Mouse.MouseDownEvent,
                    Source = elem
                });
            }
        }

        public static void xIsVisible(this UIElement ele, bool isVisible)
        {
            if (ele != null)
            {
                if (isVisible)
                {
                    ele.Visibility = Visibility.Visible;
                }
                else
                {
                    ele.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static Action EmptyDelegate = () =>
        {
        };
    }
}
