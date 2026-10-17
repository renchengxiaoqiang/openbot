using Bot.Automation.ChatDeskNs;
using Bot.ChromeNs;
using Bot.Common;
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Controls;
using BotLib.Wpf.Extensions;
using DbEntity;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Bot.AssistWindow.Widget.Right.Goods
{
	public partial class CtlGoodsList : UserControl
	{
		public EventHandler<GoodsSelectedEventArgs> EvGoodsSelected;
		private bool _isShowingPage;
		private int _goodsPerPage;
		private List<ZnkfItem> _gilist;
        private int _selectIndex;

		public CtlGoodsList()
		{
			_isShowingPage = false;
			_goodsPerPage = -1;
			InitializeComponent(); 
            pager.EvPageNumberChanged += pager_EvPageNumberChanged;
			lstGoods.SelectionChanged += lstGoods_SelectionChanged;
			lstGoods.xSetItemStrech();
		}

		private void lstGoods_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var ctlOneGoods = lstGoods.SelectedItem as CtlOneGoods;
            if (EvGoodsSelected != null)
			{
                EvGoodsSelected(ctlOneGoods, new GoodsSelectedEventArgs
                {
                    SelectedGoods = ((ctlOneGoods != null) ? ctlOneGoods.GoodsInfo : null)
                });
			}
		}

        private void pager_EvPageNumberChanged(object sender, PageChangedEventArgs e)
		{
			ShowPage(e.NewPageNo);
		}

		private async Task ShowPage(int page)
		{
			Util.Assert(page > 0);
            var qn = QN.CurQN;
            if (qn != null)
            {
                var res = await qn.GetItems(1);
				ShowPage(res.data.data);

				var pageCount = res.data.totalCount + res.data.pageSize -1 / res.data.pageSize;
                pager.Init(pageCount, 1);
                tblkGoodsCount.Text = string.Format("共{0}个宝贝", res.data.totalCount);
            }

        }


		private void ShowPage(List<ZnkfItem> gilist)
		{
			if (!_isShowingPage)
			{
				_isShowingPage = true;
				try
				{
					lstGoods.Items.Clear();
					foreach (var gi in gilist)
					{
						var ctlOneGoods = new CtlOneGoods(gi);
						ctlOneGoods.HorizontalAlignment = HorizontalAlignment.Stretch;
						lstGoods.Items.Add(ctlOneGoods);
					}
                    int selectIndex = 0;
					if (_selectIndex >= 0)
					{
                        selectIndex = _selectIndex;
						_selectIndex = -1;
					}
                    SelectGoods(selectIndex);
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
				_isShowingPage = false;
			}
		}

		private void SelectGoods(int idx)
		{
			if (idx >= 0 && lstGoods.Items.Count > idx)
			{
				lstGoods.SelectedIndex = idx;
			}
		}

		public void Init()
		{
			ShowPage(1);
        }

		public int GoodsPerPage
		{
			get
			{
				if (_goodsPerPage <= 0)
				{
                    ResetGoodsPerPage();
				}
				return _goodsPerPage;
			}
			set
			{
				if (value != _goodsPerPage)
				{
					_goodsPerPage = value;
					if (_goodsPerPage < 3)
					{
						_goodsPerPage = 3;
					}
					if (_goodsPerPage > 20)
					{
						_goodsPerPage = 20;
					}
				}
			}
		}

        private void ResetGoodsPerPage()
		{
			GoodsPerPage = (int)Math.Floor(lstGoods.ActualHeight / 70.0);
			if (GoodsPerPage < 1)
			{
				GoodsPerPage = 1;
			}
		}

        public void FocusGoods(long numiid)
		{
			int findIndex = _gilist.FindIndex(k=>k.itemId == numiid);
			if (findIndex >= 0)
			{
				int pageNo = findIndex / _goodsPerPage + 1;
				findIndex -= (pageNo - 1) * _goodsPerPage;
				if (pager.PageNo != pageNo)
				{
					_selectIndex = findIndex;
					pager.PageNo = pageNo;
				}
				else
				{
					SelectGoods(findIndex);
				}
			}
		}


		private void Button_RefreshGoodsInfo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				
			}
			catch (Exception ex)
			{
				MsgBox.ShowErrDialog(ex.Message);
				Log.Exception(ex);
			}
		}

	}
}
