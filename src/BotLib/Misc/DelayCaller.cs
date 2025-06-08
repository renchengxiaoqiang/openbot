using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;

namespace BotLib.Misc
{
    public class DelayCaller
    {
        private static ConcurrentDictionary<DelayCaller, bool> _set = new ConcurrentDictionary<DelayCaller, bool>();
        private Action _act;
        private object _synobj = new object();
        private bool _isBusy = false;
        private int _delayMs;
        private DateTime _callTime = DateTime.MaxValue;
        private Timer _timer;
        public static void CallAfterDelayInUIThread(Action act, int delayMs)
        {
            CallAfterDelay(act, delayMs, true);
        }

        public static void CallAfterDelay(Action act, int delayMs, bool runInUiThread)
        {
            if (delayMs > 0)
            {
                DelayCaller dc = null;
                dc = new DelayCaller(()=>
                {
                    act();
                    _set.xTryRemove(dc);
                }, delayMs, runInUiThread);
                _set.TryAdd(dc, true);
                dc.CallAfterDelay();
            }
            else if (runInUiThread)
            {
                DispatcherEx.xInvoke(act);
            }
            else
            {
                act();
            }
        }

        public DelayCaller(Action act, int delayMs, bool useUIThread)
        {
           _act = (useUIThread ? new Action(()=>
            {
                DispatcherEx.xInvoke(act);
            }) : act);
           _delayMs = delayMs;
        }

        public void CallAfterDelay()
        {
            lock (_synobj)
            {
               _callTime = DateTime.Now;
                if (_timer == null)
                {
                    int dut =_delayMs / 3;
                    if (dut <= 0)
                    {
                        dut = 1;
                    }
                   _timer = new Timer(new TimerCallback(this.Callback), null, dut, dut);
                }
            }
        }

        private void Callback(object state)
        {
            DateTime callTime;
            lock (_synobj)
            {
                if (this._isBusy || !this._callTime.xIsTimeElapseMoreThanMs(this._delayMs))
                {
                    return;
                }
                callTime =_callTime;
               _isBusy = true;
            }
           _act();
            lock (_synobj)
            {
                if (callTime ==_callTime)
                {
                   _callTime = DateTime.MaxValue;
                   _timer.Dispose();
                   _timer = null;
                }
               _isBusy = false;
            }
        }
    }
}
