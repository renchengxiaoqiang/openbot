using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow.Widget.Robot
{
	public partial class CtlImage : UserControl
    {
        private string _imgUrl;
        private string _goodsUrl;
        private WndAssist _wndDontUse;
        private bool _isMouseInsideImageControl;
        private DateTime _mouseEnterTime;

		public CtlImage()
		{
			_isMouseInsideImageControl = false;
			_mouseEnterTime = DateTime.MaxValue;
			InitializeComponent();
		}

		public void Init(string imgUrl, string goodsUrl)
		{
			_imgUrl = imgUrl;
			_goodsUrl = goodsUrl;
			imgGoods.Source = DownloadImage(imgUrl);
		}

        private static string GetImageFileName(string url)
        {
            string subDirOfData = PathEx.GetSubDirOfData("imgcache");
            if (!Directory.Exists(subDirOfData))
            {
                Directory.CreateDirectory(subDirOfData);
            }
            return Path.Combine(subDirOfData, url.GetHashCode().ToString());
        }

        private BitmapImage DownloadImage(string url)
        {
            var filename = GetImageFileName(url);
            FileEx.DeleteWithoutException(filename);
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(url, filename);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    Log.Info("无法下载图片，url=" + url);
                }
            }
            BitmapImage img = BitmapImageEx.CreateFromFile(filename, 3);
			return img;
        }

        private void imgGoods_MouseDown(object sender, MouseButtonEventArgs e)
		{
			
		}

		private WndAssist Wnd
		{
			get
			{
				if (_wndDontUse == null)
				{
					_wndDontUse = this.xFindAncestor<WndAssist>();
				}
				return _wndDontUse;
			}
		}

		private void imgGoods_MouseEnter(object sender, MouseEventArgs e)
		{
			OnMouseEnter();
		}

		private void OnMouseEnter()
		{
		}

		private void OnMouseLeave()
		{
			try
			{
				_isMouseInsideImageControl = false;
				_mouseEnterTime = DateTime.MaxValue;
				if (Wnd != null)
				{
					Wnd.imgBig.Source = null;
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		private void imgGoods_MouseLeave(object sender, MouseEventArgs e)
		{
			OnMouseLeave();
		}

	}
}
