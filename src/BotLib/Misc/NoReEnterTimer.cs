using BotLib.BaseClass;
using BotLib.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class NoReEnterTimer : Disposable
    {
        private ConcurrentDictionary<Action, RunInfo> _actDict = new ConcurrentDictionary<Action, RunInfo>();
        private Action _act;
        private Thread _thread;
        private int _sleepMs;
        public static bool IsAppClosed = false;
        private bool _isStop = false;

        public NoReEnterTimer(Action act, int intervalMs, int delayMs = 0)
        {
            AddAction(act, intervalMs, delayMs);
            _thread = ThreadEx.Run(new ThreadStart(Loop), true, null);
        }

        public void AddAction(Action act, int intervalMs, int delayMs = 0)
        {
            AddAction(new RunInfo(act, intervalMs, delayMs));
        }

        public void AddAction(RunInfo ri)
        {
            Trace.Assert(ri.IntervalMs > 0);
            if (_actDict.ContainsKey(ri.Act))
            {
                throw new Exception("Action已添加");
            }
            _actDict[ri.Act] = ri;
            UpdateSleepInterval();
        }

        public void RemoveAction(Action act)
        {
            if (!base.IsDisposed)
            {
                _actDict.xTryRemove(act);
                UpdateSleepInterval();
            }
        }

        private void UpdateSleepInterval()
        {
            int sleepMs;
            if (_actDict != null)
            {
                sleepMs = _actDict.Min(k => k.Value.GetLoopDelay());
            }
            else
            {
                sleepMs = 10000;
            }
            _sleepMs = sleepMs;
            if (_sleepMs <= 0)
            {
                _sleepMs = 1;
            }
        }

        private void Loop()
        {
            while (!_isStop && !IsAppClosed)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    bool runIfNeed = false;
                    foreach (var kv in _actDict)
                    {
                        if (_isStop)
                        {
                            break;
                        }
                        try
                        {
                            runIfNeed = kv.Value.RunIfNeed();
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                    if (!_isStop)
                    {
                        if (runIfNeed)
                        {
                            UpdateSleepInterval();
                        }
                        int diffMs = (int)(DateTime.Now - now).TotalMilliseconds;
                        if (diffMs < 0)
                        {
                            diffMs = 0;
                        }
                        int sleepMs = _sleepMs - diffMs;
                        if (sleepMs <= 0)
                        {
                            sleepMs = 100;
                        }
                        Thread.Sleep(sleepMs);
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    if (ex is ThreadAbortException)
                    {
                        _isStop = true;
                    }
                }
            }
        }

        protected override void CleanUp_Managed_Resources()
        {
            _isStop = true;
        }

        public void Stop()
        {
            base.Dispose();
        }

        public class RunInfo
        {
            public Action Act;
            private DateTime _preRunTime = DateTime.MinValue;
            private DateTime _createTime = DateTime.Now;
            private int _delayMs;
            public int IntervalMs { get; private set; }

            public int GetLoopDelay()
            {
                int rt;
                if (_delayMs == 0 || _preRunTime != DateTime.MinValue)
                {
                    rt = IntervalMs / 2;
                }
                else
                {
                    rt = Math.Min(IntervalMs, _delayMs) / 2;
                }
                return rt;
            }

            public RunInfo(Action act, int interval, int delayMs)
            {
                Act = act;
                IntervalMs = interval;
                _delayMs = delayMs;
            }

            public bool RunIfNeed()
            {
                bool rt = false;
                if ( NeedRun())
                {
                    try
                    {
                        rt = (_preRunTime == DateTime.MinValue && _delayMs != 0);
                        _preRunTime = DateTime.Now;
                        Act();
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                }
                return rt;
            }

            private bool NeedRun()
            {
                return _createTime.xIsTimeElapseMoreThanMs(_delayMs) && _preRunTime.xIsTimeElapseMoreThanMs(IntervalMs);
            }

        }
    }
}
