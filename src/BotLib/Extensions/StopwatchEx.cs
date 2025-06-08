using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public class StopwatchEx
    {
        private static Stopwatch _watch = new Stopwatch();

        static StopwatchEx()
        {
            _watch.Start();
        }

        public static long ElapsedTicks
        {
            get
            {
                return _watch.Elapsed.Ticks;
            }
        }
    }
}
