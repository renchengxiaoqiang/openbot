using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BotLib.Wpf.Extensions
{
    public static class DispatcherEx
    {
        public static void xInvoke(Action act)
        {
            if (((Application.Current != null) ? Application.Current.Dispatcher.Thread : null) == Thread.CurrentThread)
            {
                act();
            }
            else
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(act, new object[0]);
                }
            }
        }

        public static void xInvoke(Action act, DispatcherPriority priority)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(act, priority, new object[0]);
            }
        }

        public static void xInovkeLowestPriority(Action act)
        {
            DispatcherEx.xInvoke(act, DispatcherPriority.ApplicationIdle);
        }

        public static void xBeginInvoke(Action act)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(act, new object[0]);
            }
        }

        public static void WaitWithDoEvents(int waitMs, Func<bool> breakTest = null, int sleepMs = 50)
        {
            if (sleepMs > waitMs / 2)
            {
                sleepMs = waitMs / 2;
            }
            Util.Assert(sleepMs > 0);
            var now = DateTime.Now;
            while (!now.xIsTimeElapseMoreThanMs(waitMs) && (breakTest == null || !breakTest()))
            {
                Thread.Sleep(sleepMs);
                DoEvents();
            }
        }

        public static void DoEvents()
        {
            try
            {
                DispatcherFrame dispatcherFrame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(DispatcherEx.ExitFrame), dispatcherFrame);
                Dispatcher.PushFrame(dispatcherFrame);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
    }
}
