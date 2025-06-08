using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Bot.Common.Windows;
using BotLib.Wpf.Extensions;

namespace Bot.Common.EmojiInputer
{
	public partial class WndEmojiInputer : EtWindow
    {
        private int _showingIndex;

		public WndEmojiInputer()
		{
			_showingIndex = -1;
			InitializeComponent();
			Point point = new Point(30.0, 30.0);
			point = PointEx.xConvertPhisicalToLogical(point);
			rect.Width = point.X;
			rect.Height = point.Y;
			rect.RadiusX = 5.0;
			rect.RadiusY = 5.0;
			rect.Stroke = Brushes.Gray;
			tblk.Background = Brushes.Wheat;
		}

		private void img_MouseMove(object sender, MouseEventArgs e)
		{
			int idx = GetEmojisIndex(e.GetPosition(canvas));
			if (idx < 0)
			{
                ShowingDesc();
			}
			else
			{
                ShowingDesc(idx);
			}
		}

		private int GetEmojisIndex(Point p)
		{
			int idx = -1;
            double width = img.ActualWidth / 13.28;
			double height = img.ActualHeight / 8.28;
            double spaceing = 25.0;
            int row = (int)(p.X / width);
			int col = (int)(p.Y / height);
            double minWidth = (double)row * width + 12.0;
            double maxWidth = minWidth + spaceing;
			double minHeight = (double)col * height + 10.0;
            double maxHeight = minHeight + spaceing;
			if (p.X >= minWidth && p.X < maxWidth && p.Y >= minHeight && p.Y < maxHeight)
			{
				idx = col * 13 + row;
			}
			return idx;
		}

        private void ShowingDesc()
		{
            ShowingDesc(-1);
		}

        private void ShowingDesc(int idx)
		{
			if (idx > 97 || idx < 0)
			{
				idx = -1;
			}
			if (idx != _showingIndex)
			{
				_showingIndex = idx;
				if (idx < 0)
				{
					tblk.Visibility = Visibility.Collapsed;
					rect.Visibility = Visibility.Collapsed;
				}
				else
				{
					int row = idx / 13;
					int col = idx % 13;
					double width = img.ActualWidth / 13.28;
					double height = img.ActualHeight / 8.28;
					double spaceing = 25.0;
					Point point = new Point((double)col * width + 6.0, (double)row * height + 6.0);
					rect.SetValue(Canvas.LeftProperty, point.X);
					rect.SetValue(Canvas.TopProperty, point.Y);
					rect.Visibility = Visibility.Visible;
					tblk.Text = EmojiHelper.GetDesc(idx);
					if (col < 10)
					{
						tblk.SetValue(Canvas.LeftProperty, point.X);
					}
					else
					{
                        tblk.SetValue(Canvas.RightProperty, point.X + spaceing);
					}
                    tblk.SetValue(Canvas.TopProperty, point.Y + VisualEx.YRatioOfLogicalVsPhysical * spaceing);
					tblk.Visibility = Visibility.Visible;
				}
			}
		}

		private string GetEmojiText()
		{
			string txt = "";
			if (_showingIndex >= 0 && _showingIndex < 98)
			{
				txt = EmojiHelper.GetText(_showingIndex);
			}
			return txt;
		}

		private void img_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Close();
		}

		public static void MyShow(Window owner, Action<string> callback)
		{
			var wnd = new WndEmojiInputer();
            wnd.FirstShow(null, owner, () => {
				var emojiText = wnd.GetEmojiText();
                if (callback != null)
					callback(emojiText);
            });
		}

	}
}
