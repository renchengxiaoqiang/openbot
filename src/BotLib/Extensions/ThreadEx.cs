using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class ThreadEx
    {
        public static Thread Run(ThreadStart ts, bool isBackground = true, ThreadStart callback = null)
        {
            var t = new Thread(ts);
            t.IsBackground = isBackground;
            if (callback != null)
            {
                ts = (ThreadStart)Delegate.Combine(ts, callback);
            }
            t.Start();
            return t;
        }
    }
}
