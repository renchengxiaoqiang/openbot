using BotLib.BaseClass;
using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using BotLib.Extensions;
using BotLib;
using Bot.AssistWindow.NotifyIcon;
using Bot.Automation.ChatDeskNs.Automators;
using Bot.Common;
using Bot.Automation.ChatDeskNs;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bot.ControllerNs
{
    public class DeskScanner : Disposable
    {
        private const int ScanIntervalMs = 1000;
        private static NoReEnterTimer _timer;
        private static bool _hadDetectSellerEver;
        private static bool _hadTipNoSellerEver;

        static DeskScanner()
        {
            _hadDetectSellerEver = false;
            _hadTipNoSellerEver = false;
        }

        public static void LoopScan()
        {
            _timer = new NoReEnterTimer(Loop, ScanIntervalMs, 0);
        }

        private static async void Loop()
        {
            try
            {
                //item1 新的千牛接待窗口, item2 关闭的接待窗口
                var wnd = GetOpenedSingleChatWnd();
                var newChatWnd = wnd.Item1;
                var closedChatWnd = wnd.Item2;

                if (newChatWnd == null)
                {
                    await Task.Delay(5000);
                    return;
                }

                //检测到退出千牛，程序释放资源
                if (newChatWnd != null && closedChatWnd != null
                    && newChatWnd.Pid != wnd.Item2.Pid)
                {
                    var desk = Desk.Inst;
                    if (desk != null)
                    {
                        desk.Dispose();
                        desk = null;
                    }
                }

                
                if (Desk.Inst == null || newChatWnd != null && Desk.Inst.ProcessId != newChatWnd.Pid)
                {
                    Desk.Create(newChatWnd);
                }

                
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private static (QnChatWnd, QnChatWnd) GetOpenedSingleChatWnd()
        {
            var wnd = QnAccountFinderFactory.Finder.GetSingleChatWnd();
            DetectQianniu(wnd.Item1);
            return wnd;
        }

        private static void DetectQianniu(QnChatWnd chatWnd)
        {
            if (chatWnd == null)
            {
                if (!_hadDetectSellerEver && !_hadTipNoSellerEver)
                {
                    var msg = string.Empty;
                    if (Process.GetProcessesByName("AliWorkbench").Length < 1)
                    {
                        msg = string.Format("需要打开千牛【接待窗口】,{0}才能起作用", Params.AppName);
                    }
                    else
                    {
                        msg = string.Format("需要运行千牛，并打开接待窗口，{0}才能起作用!!", Params.AppName);
                    }
                    _hadTipNoSellerEver = true;
                    MsgBox.ShowTrayTip(msg, "没有检测到【千牛接待窗口】", 30);
                }
            }
            else
            {
                _hadDetectSellerEver = true;
            }
        }

        protected override void CleanUp_Managed_Resources()
        {
            _timer.Stop();
            _timer.Dispose();
        }

    }

}
