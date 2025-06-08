using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BotLib.Wpf.Extensions
{
    public static class ListBoxEx
    {
        public static void xSetItemStrech(this ListBox lb)
        {
            lb.ItemContainerStyle = new Style(typeof(ListBoxItem))
            {
                Setters = 
				{
					new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch)
				}
            };
        }

        public static ListBoxItem GetSelectedListBoxItem(this ListBox lb)
        {
            ListBoxItem it= null;
            if (lb.SelectedItem != null)
            {
                it = (lb.ItemContainerGenerator.ContainerFromItem(lb.SelectedItem) as ListBoxItem);
            }
            return it;
        }
    }
}
