using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using BotLib.Extensions;

namespace BotLib.Wpf.Extensions
{
    public static class TabControlEx
    {
        public static void xHoverSelect(this TabItem ti)
        {
            TabControlEx.HoverSelectInfo.SetHoverSelect(ti);
        }

        public static TabItem xGetSelectedItem(this TabControl tc)
        {
            TabItem it = null;
            if ( tc.SelectedIndex >= 0)
            {
                it = (tc.Items[tc.SelectedIndex] as TabItem);
            }
            return it;
        }

        private class HoverSelectInfo
        {
            private TabItem PreEnterTabItem;
            private DelayCaller DelayCall;
            private TabControl Parent;
            private const int DelayMs = 250;
            private static Dictionary<TabControl, HoverSelectInfo> _dict = new Dictionary<TabControl, HoverSelectInfo>();
            private static DateTime _preRemoveTime = DateTime.Now;

            public static void SetHoverSelect(TabItem ti)
            {
                ti.MouseEnter -= HoverSelectInfo.OnMouseEnterTabItem;
                ti.MouseEnter += HoverSelectInfo.OnMouseEnterTabItem;
            }

            private HoverSelectInfo(TabControl parent)
            {
                this.Parent = parent;
                this.DelayCall = new DelayCaller(new Action(this.SelectTabItemIfHoverOverTime), 250, true);
            }

            private void SelectTabItemIfHoverOverTime()
            {
                if (this.PreEnterTabItem != null)
                {
                    bool isMouseOver = this.PreEnterTabItem.IsMouseOver;
                    if (isMouseOver)
                    {
                        this.Parent.SelectedItem = this.PreEnterTabItem;
                        this.PreEnterTabItem = null;
                    }
                }
            }

            private static void OnMouseEnterTabItem(object sender, MouseEventArgs e)
            {
                TabItem tabItem = sender as TabItem;
                if (!tabItem.IsSelected)
                {
                    TabControl tabControl = tabItem.xFindAncestor<TabControl>();
                    if (tabControl != null)
                    {
                        if (!HoverSelectInfo._dict.ContainsKey(tabControl))
                        {
                            HoverSelectInfo._dict[tabControl] = new HoverSelectInfo(tabControl);
                            tabControl.SelectionChanged -= HoverSelectInfo.OnTabConrolSelectionChanged;
                            tabControl.SelectionChanged += HoverSelectInfo.OnTabConrolSelectionChanged;
                        }
                        HoverSelectInfo hoverSelectInfo = HoverSelectInfo._dict[tabControl];
                        hoverSelectInfo.PreEnterTabItem = tabItem;
                        hoverSelectInfo.DelayCall.CallAfterDelay();
                    }
                }
                TabControlEx.HoverSelectInfo.RemoveDetachedTabControlsIfNeed();
            }

            private static void OnTabConrolSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                TabControl tabControl = sender as TabControl;
                if (tabControl != null && HoverSelectInfo._dict.ContainsKey(tabControl))
                {
                    HoverSelectInfo._dict[tabControl].PreEnterTabItem = null;
                }
            }

            private static void RemoveDetachedTabControlsIfNeed()
            {
                if (HoverSelectInfo._preRemoveTime.xIsTimeElapseMoreThanMinute(1.0))
                {
                    HoverSelectInfo._preRemoveTime = DateTime.Now;
                    int num = HoverSelectInfo._dict.xRemoveAll(kv =>
                    {
                        TabControl tabControl = kv.Key;
                        return tabControl.xFindParentWindow() == null;
                    });
                }
            }
        }
    }
}