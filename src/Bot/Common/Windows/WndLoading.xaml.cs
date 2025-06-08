using BotLib.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BotLib.Extensions;

namespace Bot.Common.Windows
{
    public partial class WndLoading : EtWindow
    {
        public static ManualResetEventSlim _waiter;

        static WndLoading()
        {
            _waiter = new ManualResetEventSlim(true);
        }
        public WndLoading(string title)
        {
            InitializeComponent();
            Topmost = true;
            if(!string.IsNullOrEmpty(title))
                txtTitle.Text = title;
        }

        public static void ShowWaiting(string tip = "")
        {
            lock (_waiter)
            {
                Task.Factory.StartNew(() =>
                {
                    DispatcherEx.xInvoke(() =>
                    {
                        _waiter.Wait();
                        _waiter.Reset();
                        var wnd = new WndLoading(tip);
                        wnd.xShowFirstTime();
                        wnd.Closed += (sender, e) =>
                        {
                            _waiter.Set();
                        };
                    });
                }, TaskCreationOptions.LongRunning);
            }
        }

        public static void CloseWaiting()
        {
            var wnds = WindowEx.GetAppWindows<WndLoading>();
            if (wnds.xCount() > 0)
            {
                var wnd = wnds.FirstOrDefault();
                if (wnd != null)
                {
                    wnd.Close();
                }
            }
        }
    }


}
