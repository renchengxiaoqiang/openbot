using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using Bot.Automation.ChatDeskNs;
using DbEntity;
using BotLib.Db.Sqlite;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow.Widget.Right.Goods
{
	public partial class CtlGoods : UserControl
	{
		private RightPanel _rightPanel;
        private bool _isUiInited;

		public CtlGoods(RightPanel rp)
		{
			_isUiInited = false;
			InitializeComponent();
            _rightPanel = rp;
            ctlGoodsList.EvGoodsSelected += ctlGoodsList_EvGoodsSelected;
			Loaded += CtlGoods_Loaded;
		}

		private void CtlGoods_Loaded(object sender, RoutedEventArgs e)
		{
            if (!_isUiInited && this.xFindAncestor<TabItem>().IsSelected)
			{
				InitUI();
			}
		}

        private void ctlGoodsList_EvGoodsSelected(object sender, GoodsSelectedEventArgs e)
		{

		}

		public void InitUI()
		{
			if (!_isUiInited)
			{
				_isUiInited = true;
				ctlGoodsList.Init();
			}
		}
	}
}
