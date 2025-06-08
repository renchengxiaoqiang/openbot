using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BotLib.Wpf.Extensions
{
    public static class ButtonEx
	{
		public static void xPerformClick(this Button btn)
		{
			btn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
		}

		public static void xPerformClick(this ButtonBase btn)
		{
			btn.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
		}
	}
}
