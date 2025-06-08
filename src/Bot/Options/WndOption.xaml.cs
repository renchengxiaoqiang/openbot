using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Delay;
using Bot.AssistWindow;
using Bot.Common;
using Bot.Common.Account;
using Bot.Common.Db;
using Bot.Common.Windows;
using BotLib;
using BotLib.Wpf.Extensions;

namespace Bot.Options
{
	public partial class WndOption : EtWindow
	{
		private WndOption(string seller)
		{
			Seller = seller;
			InitializeComponent();
			Loaded += WndOption_Loaded;
		}

		private void WndOption_Loaded(object sender, RoutedEventArgs e)
		{
			Style style = FindResource("tabLevel1") as Style;
            CreateOpTab("AI大模型设置", new CtlRobotOptions(Seller), style);
            sbSave.ToolTip = string.Format("保存成 {0} 个人设置", Seller);
		}

		private void CreateOpTab(string tabTitle, object control, Style style)
		{
			var tabItem = new TabItem();
			var header = TextBlockEx.Create(tabTitle, new object[0]);
			tabItem.Header = header;
			tabItem.Content = control;
			tabMain.Items.Add(tabItem);
			tabItem.Style = style;
		}

		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedSettingControl != null)
			{
				SelectedSettingControl.NavHelp();
			}
		}

		private IOptions SelectedSettingControl
		{
			get
			{
                return GetSelectedSettingTabControl(tabMain);
			}
		}

        private IOptions GetSelectedSettingTabControl(TabControl tabm)
		{
            var selectedTab = tabm.SelectedContent as TabControl;
			IOptions rtOp;
            if (selectedTab == null)
			{
				rtOp = (tabm.SelectedContent as IOptions);
			}
			else
			{
                rtOp = GetSelectedSettingTabControl(selectedTab);
			}
			return rtOp;
		}

        private void TraversalOpsAndDoAction(Action<IOptions> act)
		{
            TraversalOpsAndDoAction(act, tabMain);
		}

        private void TraversalOpsAndDoAction(Action<IOptions> act, TabControl tabm)
		{
			foreach (var obj in tabm.Items)
			{
				var tabItem = (TabItem)obj;
				if ( tabItem.IsLoaded)
				{
					var tabControl = tabItem.Content as TabControl;
					if (tabControl != null)
					{
                        TraversalOpsAndDoAction(act, tabControl);
					}
					else
					{
						var options = tabItem.Content as IOptions;
						if (options != null)
						{
							act(options);
						}
						else
						{
							MsgBox.ShowErrDialog("WndOption,异常的TabItem,header=" + tabItem.Header.ToString(), null);
						}
					}
				}
			}
		}

		public static void MyShow(string seller, WndAssist owner = null, OptionEnum showPage = OptionEnum.Unknown, Action uiCallback = null)
		{
			Util.Assert(!string.IsNullOrEmpty(seller));
			var wndOp = ShowSameNickOneInstance<WndOption>(seller, ()=> {
                return new WndOption(seller);
            } , owner, true);
			if (uiCallback != null)
			{
				wndOp.Closed += (s,e)=>{
                    if(uiCallback!=null)
                        uiCallback();
                };
			}
			if (showPage > OptionEnum.Unknown)
			{
				wndOp.ShowPage(showPage);
			}
		}

		private void ShowPage(OptionEnum showPage)
		{
            TraversalOpsAndDoAction(op =>
            {
                if(showPage == op.OptionType)
                    ShowPage(op);
            });
		}

		private void ShowPage(IOptions op)
		{
			TabControl tabControl;
			for (var c = op as Control; c != null; c = tabControl)
			{
				var tabItem = c.xFindAncestor<TabItem>();
				if (tabItem == null)
				{
					break;
				}
				tabControl = tabItem.xFindAncestor<TabControl>();
				tabControl.SelectedItem = tabItem;
			}
		}

		private void btnRestoreAllPageToDef_Click(object sender, RoutedEventArgs e)
		{
            TraversalOpsAndDoAction(op =>
			{
				op.RestoreDefault();
			});
		}

		private void sbSave_Click(object sender, RoutedEventArgs e)
		{
			Save(Seller);
		}


		private void Save(string seller)
		{
			Util.Assert(!string.IsNullOrEmpty(seller));
			Hide();
            TraversalOpsAndDoAction(op =>
            {
                op.Save(seller);
            });
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void btnSaveOneShopDefOp_Click(object sender, RoutedEventArgs e)
		{
			Save(AccountHelper.GetShopDbAccount(Seller));
		}

		private void btnSaveMulitiShopDefOp_Click(object sender, RoutedEventArgs e)
		{
			Save(AccountHelper.GetPubDbAccount(Seller));
		}

		private void EtWindow_Closed(object sender, EventArgs e)
		{

		}

		private void btnRestoreCurrentPageToDef_Click(object sender, RoutedEventArgs e)
		{
			IOptions options = tabMain.SelectedContent as IOptions;
			options.RestoreDefault();
		}
	}
}
