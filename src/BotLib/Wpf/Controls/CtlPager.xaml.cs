using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BotLib.Misc;

namespace BotLib.Wpf.Controls
{
	public partial class CtlPager : UserControl
	{
		public event EventHandler<PageChangedEventArgs> EvPageNumberChanged;

        private DelayCaller _dcaller;
        private bool _collapseOnOnePage = true;
        private int _pageCount;
        private int _pageNo;

		public void Init(int pageCount, bool collapseOnOnePage = true, int pageNo = 1)
		{
			base.IsEnabled = true;
			Util.Assert(pageNo > 0 && pageNo <= pageCount);
			tblkPageCount.Visibility = Visibility.Visible;
			tboxPageNo.IsEnabled = true;
			_collapseOnOnePage = collapseOnOnePage;
			PageCount = pageCount;
			PageNo = pageNo;
			if (PageNo == pageNo)
			{
				RaisePageNoChangedEvent(pageNo, -1);
			}
		}

		public void Init(int recCount, int recPerPage, bool collapseOnOnePage = true, int pageNo = 1)
		{
			Util.Assert(recCount >= 0 && recPerPage > 0);
			int pageCount = (int)(1.0 + (double)recCount * 1.0 / (double)recPerPage);
			Init(pageCount, collapseOnOnePage, pageNo);
		}

		public void InitInfinitePageCount(bool disableOnPageChanged = true)
		{
			PageCount = int.MaxValue;
			PageNo = 1;
			tblkPageCount.Visibility = Visibility.Collapsed;
			tboxPageNo.IsEnabled = false;
		}

		public int PageCount
		{
			get
			{
				return _pageCount;
			}
			private set
			{
				Util.Assert(value > 0);
				_pageCount = value;
				tblkPageCount.Text = "/" + value;
				if (_collapseOnOnePage && value == 1)
				{
					base.Visibility = Visibility.Collapsed;
				}
				else
				{
					base.Visibility = Visibility.Visible;
				}
			}
		}

		public int PageNo
		{
			get
			{
				return _pageNo;
			}
			set
			{
				Util.Assert(value >= 0 && value <= PageCount);
				tboxPageNo.Text = value.ToString();
				if (value != _pageNo)
				{
					int pageNo = _pageNo;
					_pageNo = value;
					RaisePageNoChangedEvent(value, pageNo);
				}
			}
		}

		public CtlPager()
		{
			InitializeComponent();
			_dcaller = new DelayCaller(()=>
			{
				OnInputPageNumber();
			}, 500, true);
		}

		public void Reset()
		{
			EvPageNumberChanged = null;
			_pageNo = 0;
			_pageCount = 0;
			_collapseOnOnePage = true;
		}

		public void ShowFinished(int showRecCount, int recordPerPage)
		{
			if (PageCount >= int.MaxValue)
			{
				if (showRecCount == 0 && PageNo > 1)
				{
					Init(PageNo - 1, true, PageNo - 1);
				}
				else
				{
					if (showRecCount < recordPerPage)
					{
						Init(PageNo, true, PageNo);
					}
				}
			}
		}

		private void OnInputPageNumber()
		{
			if (!string.IsNullOrEmpty(tboxPageNo.Text.Trim()))
			{
				try
				{
					int pageNo = -1;
					try
					{
						pageNo = Convert.ToInt32(tboxPageNo.Text.Trim());
					}
					catch
					{
						pageNo = PageNo;
						Util.Beep();
					}
					if (pageNo < 1)
					{
						pageNo = 1;
						Util.Beep();
					}
					if (pageNo > PageCount)
					{
						pageNo = PageCount;
						Util.Beep();
					}
					PageNo = pageNo;
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
			}
		}

		private void RaisePageNoChangedEvent(int newPageNo, int oldPageNo)
		{
			IsEnabled = false;
			if (EvPageNumberChanged != null)
			{
				EvPageNumberChanged(this, new PageChangedEventArgs(newPageNo, oldPageNo));
			}
			IsEnabled = true;
		}

		private void Button_Prev_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo > 1)
			{
				int pageNo = PageNo;
				PageNo = pageNo - 1;
			}
			else
			{
				Util.Beep();
			}
		}

		private void Button_Next_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo < PageCount)
			{
				int pageNo = PageNo;
				PageNo = pageNo + 1;
			}
			else
			{
				Util.Beep();
			}
		}

		private void tboxPageNo_TextChanged(object sender, TextChangedEventArgs e)
		{
			_dcaller.CallAfterDelay();
		}

		private void Button_Last_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo == PageCount)
			{
				Util.Beep();
			}
			else
			{
				PageNo = PageCount;
			}
		}

		private void Button_First_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo == 1)
			{
				Util.Beep();
			}
			else
			{
				PageNo = 1;
			}
		}

		private void tboxPageNo_LostFocus(object sender, RoutedEventArgs e)
		{
			tboxPageNo.Text = PageNo.ToString();
		}

	}
}
