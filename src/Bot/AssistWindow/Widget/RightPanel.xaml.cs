using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Bot.Common;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Wpf.Extensions;
using Bot.Common.Db;
using DbEntity;
using System.IO;
using BotLib.Extensions;
using Newtonsoft.Json;
using Bot.Options;
using System.Linq;
using System.Threading.Tasks;
using Bot.AssistWindow.Widget.Robot;

namespace Bot.AssistWindow.Widget
{
	public partial class RightPanel : UserControl, IWakable
	{
		private WndAssist _wndDontUse;
		private bool _isWiden;
		private bool _isHighden;
		private bool _isSouthEast;
		public enum TabTypeEnum
		{
			Unknown,
			Logis,
			ShortCut,
			Robot,
			Goods,
			Order,
			Coupon,
			Test
		}

		private WndAssist Wnd
		{
			get
			{
				if (_wndDontUse == null)
				{
					WndAssist wnd = this.xFindParentWindow() as WndAssist;
					Init(wnd);
					Util.Assert(_wndDontUse != null);
				}
				return _wndDontUse;
			}
			set
			{
				_wndDontUse = value;
			}
		}

		public RightPanel()
		{
			_isWiden = false;
			_isHighden = false;
			_isSouthEast = false;
			InitializeComponent();
		}

		public void Init(WndAssist wnd)
		{
			if (_wndDontUse == null)
			{
				Wnd = wnd;
				var tabCsv = Params.Panel.GetRightPanelCompOrderCsv(Wnd.Desk.WndTitle);
				var tabs = tabCsv.Split(',');
				foreach (var tabName in tabs)
				{
					//var tabVisible = Params.Panel.GetPanelOptionVisible(seller, tabName);
					var tabType = GetTabType(tabName);
					var tabItem = CreateTabItem(tabType);
					if (tabItem != null)
					{
						//tabItem.xIsVisible(tabVisible);
						AddTabItem(tabItem, tabType);
					}
				}
				tabControl.SelectionChanged -= tabControl_SelectionChanged;
				tabControl.SelectionChanged += tabControl_SelectionChanged;
			}
		}

        public void ReShowAfterChangePanelOption()
		{
			Util.Assert(_wndDontUse != null);
			tabControl.Items.Clear();
			var tabCsv = Params.Panel.GetRightPanelCompOrderCsv(Wnd.Desk.WndTitle);
			var tabs = tabCsv.Split(',');
			foreach (var tabName in tabs)
			{
				var tabVisible = Params.Panel.GetPanelOptionVisible(Wnd.Desk.WndTitle, tabName);
				var tabType = GetTabType(tabName);
				var tabItem = CreateTabItem(tabType);
				if (tabItem != null)
				{
					tabItem.xIsVisible(tabVisible);
					AddTabItem(tabItem, tabType);
				}
			}
			tabControl.SelectionChanged -= tabControl_SelectionChanged;
			tabControl.SelectionChanged += tabControl_SelectionChanged;
		}

		private TabItem CreateTabItem(TabTypeEnum tabType)
		{
			TabItem tabItem = null;
			switch (tabType)
			{
                case TabTypeEnum.Robot:
                    tabItem = TabRobot();
                    break;
                default:
					break;
			}
			return tabItem;
		}

		private TabTypeEnum GetTabType(string typeName)
		{
			var tabType = TabTypeEnum.Unknown;
			switch (typeName)
			{
				case "商品":
					tabType = TabTypeEnum.Goods;
					break;
                case "机器人":
                    tabType = TabTypeEnum.Robot;
                    break;
            }
			return tabType;
		}

		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.OriginalSource == tabControl)
			{
				SetTabStyle(e);
				ActivateTab(e);
			}
		}

		private void SetTabStyle(SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var tabIt = e.AddedItems[0] as TabItem;
				if (tabIt != null)
				{
					var tb = tabIt.Header as TextBlock;
					tb.Foreground = Brushes.White;
				}
			}
			if (e.RemovedItems.Count > 0)
			{
				var tabIt = e.RemovedItems[0] as TabItem;
				if (tabIt != null)
				{
					var tb = tabIt.Header as TextBlock;
					tb.Foreground = Brushes.Black;
				}
			}
		}

        private TabItem TabRobot()
        {
            return new TabItem
            {
                Header = "机器人",
                Content = new CtlRobot(Wnd.Desk,this)
            };
        }
		private void AddTabItem(TabItem tabItem, TabTypeEnum tabType)
		{
			tabItem.Tag = tabType;
			if (!(tabItem.Header is TextBlock))
			{
				tabItem.Header = TextBlockEx.Create(tabItem.Header.ToString(), new object[0]);
			}
			tabItem.Style = (Style)FindResource("tabRightPanel");
			tabControl.Items.Add(tabItem);
		}

		public TabItem GetTabItem(TabTypeEnum tabType)
		{
			TabItem tabItem = null;
			foreach (TabItem item in tabControl.Items)
			{
				TabTypeEnum ty = (TabTypeEnum)item.Tag;
				if (ty == tabType)
				{
					tabItem = item;
					break;
				}
			}
			return tabItem;
		}

		private void rectWiden_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isWiden = true;
		}

		private void rectWiden_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_isWiden)
			{
				_isWiden = false;
				Rectangle rectangle = sender as Rectangle;
				rectangle.ReleaseMouseCapture();
				int width = (int)e.GetPosition(this).X + 5;
				SetRightPanelWidth(width, 0);
			}
		}

		private void rectWiden_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isWiden)
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					_isWiden = false;
					var captured = Mouse.Captured;
					if (captured != null)
					{
						captured.ReleaseMouseCapture();
					}
				}
				else
				{
					var rectangle = sender as Rectangle;
					rectangle.CaptureMouse();
					int width = (int)e.GetPosition(this).X + 5;
					SetRightPanelWidth(width, 5);
				}
			}
		}

		private void SetRightPanelWidth(int width, int minVal = 5)
		{
			if (width < 365)
			{
				width = 365;
			}
			if (Math.Abs(ActualWidth - (double)width) > (double)minVal)
			{
				this.xSetWidth(width);
				WndAssist.WaParams.SetRightPanelWidth(Wnd.Desk.WndTitle, width);
			}
		}

		private void rectHighden_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isHighden)
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					_isHighden = false;
					var captured = Mouse.Captured;
					if (captured != null)
					{
						captured.ReleaseMouseCapture();
					}
				}
				else
				{
					var rectangle = sender as Rectangle;
					rectangle.CaptureMouse();
					int height = (int)e.GetPosition(this).Y + 5;
				}
			}
		}


        public TabItem GetSelectedTabItem(TabTypeEnum tabType)
		{
			TabItem tabItem = null;
			foreach (TabItem tab in tabControl.Items)
			{
				var tabTypeEnum = (TabTypeEnum)tab.Tag;
				if (tabTypeEnum == tabType)
				{
					tabItem = tab;
					break;
				}
			}
			return tabItem;
		}

		private void rectHighden_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_isHighden = false;
			Rectangle rectangle = sender as Rectangle;
			rectangle.ReleaseMouseCapture();
			int height = (int)e.GetPosition(this).Y + 5;
		}

		private void rectHighden_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isHighden = true;
		}

		private void rectCorner_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isSouthEast)
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					_isHighden = false;
					var captured = Mouse.Captured;
					if (captured != null)
					{
						captured.ReleaseMouseCapture();
					}
				}
				else
				{
					Rectangle rectangle = sender as Rectangle;
					rectangle.CaptureMouse();
					int width = (int)e.GetPosition(this).X + 5;
					int height = (int)e.GetPosition(this).Y + 5;
					SetRightPanelWidth(width, 5);
				}
			}
		}

		private void rectCorner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_isSouthEast = false;
			var rectangle = sender as Rectangle;
			rectangle.ReleaseMouseCapture();
			int width = (int)e.GetPosition(this).X + 5;
			int height = (int)e.GetPosition(this).Y + 5;
			SetRightPanelWidth(width, 0);
		}

		private void rectCorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isSouthEast = true;
		}

		public void WakeUp()
		{
			IWakable wakable = tabControl.SelectedContent as IWakable;
			if (wakable != null)
			{
				wakable.WakeUp();
			}
		}

		public void Sleep()
		{
			IWakable wakable = tabControl.SelectedContent as IWakable;
			if (wakable != null)
			{
				wakable.Sleep();
			}
		}

		private void ActivateTab(SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null)
			{
				foreach (TabItem tabItem in e.AddedItems)
				{
					if (tabItem != null)
					{
						IWakable wakable = tabItem.Content as IWakable;
						if (wakable != null)
						{
							wakable.WakeUp();
						}
					}
				}
			}
			if (e.RemovedItems != null)
			{
				foreach (TabItem tabItem in e.RemovedItems)
				{
					if (tabItem != null)
					{
						IWakable wakable = tabItem.Content as IWakable;
						if (wakable != null)
						{
							wakable.Sleep();
						}
					}
				}
			}
		}

		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
			TabItem tabItem = tabControl.SelectedItem as TabItem;
			var text = "Help";
			if (tabItem != null)
			{
				switch ((TabTypeEnum)tabItem.Tag)
				{
					case TabTypeEnum.Logis:
						text = "Logis";
						break;
					case TabTypeEnum.ShortCut:
						text = "Shortcut";
						break;
					case TabTypeEnum.Robot:
						text = "Robot";
						break;
					case TabTypeEnum.Goods:
						text = "Knowledge";
						break;
					case TabTypeEnum.Order:
						text = "Trade";
						break;
				}
			}
		}

		private void btnHide_Click(object sender, RoutedEventArgs e)
        {
			Wnd.HidePanelRight();
        }

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            WndOption.MyShow(Wnd.Desk.WndTitle, Wnd, OptionEnum.Robot);
        }
    }
}
