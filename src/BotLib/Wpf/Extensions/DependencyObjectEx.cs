using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BotLib.Wpf.Extensions
{
    public static class DependencyObjectEx
    {
        public static T xFindAncestor<T>(this DependencyObject source) where T : DependencyObject
		{
			try
			{
				if (source != null)
				{
					source = GetParent(source);
					while (source != null && !(source is T))
					{
						source = GetParent(source);
					}
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			return (T)((object)source);
		}

        public static T xFindAncestorFromMe<T>(this DependencyObject source) where T : DependencyObject
		{
			try
			{
				while (source != null && !(source is T))
				{
					source = GetParent(source);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			return (T)((object)source);
		}

        public static bool xContainsDescendant(this DependencyObject parent, object x)
        {
            var dependencyObject = x as DependencyObject;
            while (dependencyObject != null)
            {
                dependencyObject = GetParent(dependencyObject);
                if (dependencyObject == parent)
                {
                    return true;
                }
            }
            return false;
        }

        private static DependencyObject GetParent(DependencyObject current)
        {
            if (current is Visual || current is Visual3D)
            {
                current = (LogicalTreeHelper.GetParent(current) ?? VisualTreeHelper.GetParent(current));
            }
            else
            {
                current = LogicalTreeHelper.GetParent(current);
            }
            return current;
        }

        public static Window xFindParentWindow(this DependencyObject source)
        {
            return source.xFindAncestorFromMe<Window>();
        }
    }
}
