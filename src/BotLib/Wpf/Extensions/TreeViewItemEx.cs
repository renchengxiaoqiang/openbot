using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BotLib.Wpf.Extensions
{
    public static class TreeViewItemEx
    {
        public static void xAutoExpandChildren(this TreeViewItem item, int level = 1)
        {
            item.Expanded += (sender, e) =>
            {
                var it = sender as TreeViewItem;
                ExpandChildren(it, level);
            };
        }

        private static void ExpandChildren(TreeViewItem item, int level)
        {
            if (item != null)
            {
                foreach (TreeViewItem it in item.Items)
                {
                    it.IsExpanded = true;
                    if (level > 1)
                    {
                        TreeViewItemEx.ExpandChildren(it, level - 1);
                    }
                }
            }
        }

        public static TreeViewItem xCreateByHeader(object header)
        {
            return new TreeViewItem
            {
                Header = header
            };
        }

        public static TreeViewItem xParentTreeViewItem(this TreeViewItem tv)
        {
            return tv.xFindAncestor<TreeViewItem>();
        }

        public static void xBrindLastItemIntoView(this TreeView tv, bool isExpand = false)
        {
            if (tv != null && tv.Items.Count > 0)
            {
                var it = tv.Items[tv.Items.Count - 1] as TreeViewItem;
                it.BringIntoView();
                if (isExpand)
                {
                    it.IsExpanded = true;
                }
            }
        }

        public static TreeViewItem xAppend(this TreeViewItem parent, object header, object tag = null)
        {
            var it = new TreeViewItem();
            if (header is string)
            {
                header = new TextBlock
                {
                    Text = (header as string)
                };
            }
            it.Header = header;
            it.Tag = tag;
            parent.Items.Add(it);
            return it;
        }

        public static void xTraverseDescendant(this TreeViewItem item, Action<TreeViewItem> act)
        {
            if (((item != null) ? item.Items : null) != null)
            {
                foreach (TreeViewItem it in item.Items)
                {
                    if ( it != null)
                    {
                        act(it);
                        it.xTraverseDescendant(act);
                    }
                }
            }
        }

        public static bool xTraverseDescendant(this TreeViewItem item, Func<TreeViewItem, bool> func)
        {
            bool rt = true;
            if (((item != null) ? item.Items : null) != null)
            {
                foreach (TreeViewItem it in item.Items)
                {
                    if (it != null)
                    {
                        if (!func(it) || !it.xTraverseDescendant(func))
                        {
                            rt = false;
                            break;
                        }
                    }
                }
            }
            return rt;
        }

        public static void xRemoveFromParent(this TreeViewItem item)
        {
            item.xGetParentItemCollection().Remove(item);
        }

        public static ItemCollection xGetParentItemCollection(this TreeViewItem item)
        {
            var tv = item.Parent as TreeView;
            ItemCollection items;
            if (tv != null)
            {
                items = tv.Items;
            }
            else
            {
                var it = item.Parent as TreeViewItem;
                items = it.Items;
            }
            return items;
        }

        public static TreeViewItem xAppendLightGrayTextItem(this TreeViewItem parent, string format, params object[] args)
        {
            TextBlock header = TextBlockEx.CreateWithColor(string.Format(format, args), Brushes.LightGray, null);
            return parent.xAppend(header, null);
        }

        public static TreeViewItem xAppendTextItem(this TreeViewItem parent, string format, params object[] args)
        {
            TextBlock header = TextBlockEx.Create(format, args);
            return parent.xAppend(header, null);
        }

        public static TreeViewItem xGetChildAsTreeViewItem(this TreeViewItem tvi, int i)
        {
            return tvi.Items[i] as TreeViewItem;
        }

        public static int xGetIndent(this TreeViewItem tvi)
        {
            return tvi.xGetLevel() * 19 + 3;
        }

        public static int xGetIndent(int level)
        {
            return level * 19 + 3;
        }

        public static int xGetLevel(this TreeViewItem tvi)
        {
            int level = 1;
            while (tvi.Parent is TreeViewItem)
            {
                level++;
                tvi = (tvi.Parent as TreeViewItem);
            }
            return level;
        }
    }
}
