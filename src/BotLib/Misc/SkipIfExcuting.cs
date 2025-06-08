using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public static class SkipIfExcuting
    {
        private static object _synObj = new object();
        private static Dictionary<Action, bool> _dict = new Dictionary<Action, bool>();

        public static void Excute(Action act)
        {
            lock (_synObj)
            {
                if (IsExcuting(act))
                {
                    return;
                }
                _dict[act] = true;
            }
            try
            {
                act();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            finally
            {
                lock (_synObj)
                {
                    _dict[act] = false;
                }
            }
        }

        private static bool IsExcuting(Action act)
        {
            return _dict.ContainsKey(act) && SkipIfExcuting._dict[act];
        }

        private static void Print(string s)
        {
            Util.WritaTrace(string.Format("{2}.threadid={0},taskid={1}", Thread.CurrentThread.ManagedThreadId, Task.CurrentId, s));
        }

    }
}
