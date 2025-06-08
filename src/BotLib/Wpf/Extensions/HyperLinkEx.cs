using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace BotLib.Wpf.Extensions
{
    public static class HyperLinkEx
    {
        public static void xPerformClick(this Hyperlink lk)
        {
            lk.RaiseEvent(new RoutedEventArgs(Hyperlink.ClickEvent));
        }

        public static Hyperlink Create(string s, RoutedEventHandler onClicked = null, object tag = null)
        {
            Hyperlink hyperlink = new Hyperlink();
            hyperlink.Focusable = false;
            hyperlink.Inlines.Add(s);
            if (onClicked != null)
            {
                hyperlink.Click += onClicked;
            }
            if (tag != null)
            {
                hyperlink.Tag = tag;
            }
            return hyperlink;
        }
    }
}
