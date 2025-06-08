using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BotLib.Wpf.Extensions
{
    public static class TreeViewEx
    {
        public const int LeftMargin = 3;
        public const int HeaderIndent = 19;

        public static TreeViewItem xAppend(this TreeView tv, object header, object tag = null)
        {
            TreeViewItem it = new TreeViewItem();
            it.Header = header;
            it.Tag = tag;
            tv.Items.Add(it);
            return it;
        }

        public static TreeViewItem CreateByHeader(object header)
        {
            return new TreeViewItem
            {
                Header = header
            };
        }

        public static void xTraverse(this TreeView tv, Action<TreeViewItem> act)
        {
            foreach (TreeViewItem it in tv.Items)
            {
                act(it);
                it.xTraverseDescendant(act);
            }
        }

        public static void xTraverse(this TreeView tv, Func<TreeViewItem, bool> func)
        {
            foreach (TreeViewItem it in tv.Items)
            {
                if (!func(it) || !it.xTraverseDescendant(func))
                {
                    break;
                }
            }
        }

        public static TreeViewItem xInsertAt(this TreeView tv, int idx, object header)
        {
            TreeViewItem it = new TreeViewItem();
            it.Header = header;
            tv.Items.Insert(idx, it);
            return it;
        }

        public static TreeViewItem xGetChildAsTreeViewItem(this TreeView tv, int i)
        {
            return tv.Items[i] as TreeViewItem;
        }

        public static void xSetRightClickSelectTreeviewItem(this TreeView tv)
        {
            tv.PreviewMouseRightButtonDown -= Tv_PreviewMouseRightButtonDown;
            tv.PreviewMouseRightButtonDown += Tv_PreviewMouseRightButtonDown;
        }

        private static void Tv_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var dependencyObject = e.OriginalSource as DependencyObject;
                var it = (dependencyObject != null) ? dependencyObject.xFindAncestorFromMe<TreeViewItem>() : null;
                if (it != null)
                {
                    it.IsSelected = true;
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}
