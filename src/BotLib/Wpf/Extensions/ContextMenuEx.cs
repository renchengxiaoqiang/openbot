using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BotLib.Wpf.Extensions
{
    public static class ContextMenuEx
    {
        public static MenuItem xGetMenuItemByName(this ContextMenu menu, string name)
        {
            MenuItem it = null;
            if (menu != null && menu.Items != null && menu.Items.Count > 0)
            {
                foreach (var itemobj in menu.Items)
                {
                    var item = itemobj as MenuItem;
                    if (item == null) continue;
                    if ( ((item != null) ? item.Name : null) == name)
                    {
                        it = item;
                        break;
                    }
                }
            }
            return it;
        }

        public static void xSetMenuItemVisibilityByTag(this ContextMenu menu, string tag, bool isVisible)
        {
            try
            {
                MenuItem ele = menu.xGetMenuItemByTag(tag);
                ele.xIsVisible(isVisible);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static MenuItem xGetMenuItemByTag(this ContextMenu menu, string tag)
        {
            MenuItem it = null;
            if (menu != null && menu.Items != null && menu.Items.Count > 0)
            {
                foreach (var obj in menu.Items)
                {
                    var item = obj as MenuItem;
                    if (((item != null) ? item.Tag : null) as string == tag)
                    {
                        it = item;
                        break;
                    }
                }
            }
            return it;
        }
    }
}
