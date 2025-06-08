using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class LockEx
    {
        public static bool TryLock(object obj, int timeoutMs, Action act)
        {
            bool rt = false;
            if (Monitor.TryEnter(obj, timeoutMs))
            {
                try
                {
                    rt = true;
                    if (act != null)
                    {
                        act();
                    }
                }
                finally
                {
                    Monitor.Exit(obj);
                }
            }
            return rt;
        }

        public static bool TryLockMultiTime(object obj, int timeoutMs, Action act, int maxtry = 5, int sleepMs = 10)
        {
            bool rt = false;
            Util.Assert(maxtry > 0 && sleepMs > 0);
            int tryCnt = 0;
            while (tryCnt < maxtry && !rt)
            {
                if (Monitor.TryEnter(obj, timeoutMs))
                {
                    try
                    {
                        rt = true;
                        if (act != null)
                        {
                            act();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(obj);
                    }
                }
                else
                {
                    Thread.Sleep(sleepMs);
                }
                tryCnt++;
            }
            return rt;
        }
    }
}
