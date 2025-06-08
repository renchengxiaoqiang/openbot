using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotLib;
using BotLib.Wpf.Extensions;
using BotLib.Extensions;
using System.Threading;

namespace Bot.Common.Windows
{
    public partial class WndTrayTip : Window
    {
        private string _showText;
        private int _showSecond;
        public static ManualResetEventSlim _waiter;

        static WndTrayTip()
        {
            _waiter = new ManualResetEventSlim(true);
        }


        public WndTrayTip(string showText, string title, int showSecond)
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(title))
            {
                Title = Params.AppName;
            }
            else
            {
                Title = title + " --- " + Params.AppName;
            }
            _showText = (showText ?? "");
            _showSecond = showSecond;
            Left = SystemParameters.PrimaryScreenWidth;
            Top = SystemParameters.PrimaryScreenHeight;
            Loaded += new RoutedEventHandler(WndTrayTip_Loaded);
            Closed += new EventHandler(WndTrayTip_Closed);
            if (_showSecond > 0)
            {
                Util.CallOnceAfterDelayInUiThread(() => Close(), showSecond * 1000);
            }
        }

        private void WndTrayTip_Closed(object sender, EventArgs e)
        {
            _waiter.Set();
        }

        private void WndTrayTip_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom - Height;
            SetContent(_showText);
            Show();
        }

        private void SetContent(string showText)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.AddRange(InlineEx.ConvertTextToInlineConsiderUrl(showText));
            FlowDocument flowDocument = new FlowDocument();
            flowDocument.Blocks.Add(paragraph);
            viewer.Document = flowDocument;
        }

        public static void Close(string closeKey)
        {
            var wnds = WindowEx.GetAppWindows<WndTrayTip>();
            if (wnds.xCount() > 0)
            {
                var wnd = wnds.FirstOrDefault(k => k.Tag.ToString() == closeKey);
                if (wnd != null)
                {
                    wnd.Close();
                }
            }
        }

        public static void ShowTrayTip(string msg, string title, int showSecond = 30, string closeKey = null, Action onClosed = null)
        {
            lock (_waiter)
            {
                Task.Factory.StartNew(() =>
                {
                    DispatcherEx.xInvoke(() =>
                    {
                        _waiter.Wait();
                        _waiter.Reset();
                        var wnd = new WndTrayTip(msg, title, showSecond);
                        wnd.Tag = closeKey;
                        wnd.Show();
                        wnd.Closed += (sender, e) =>
                        {
                            if (onClosed != null) onClosed();
                        };
                    });
                }, TaskCreationOptions.LongRunning);
            }
        }

        public static void ShowTrayTip(string msg, string title = null)
        {
            ShowTrayTip(msg, title,15);
        }
    }
}
