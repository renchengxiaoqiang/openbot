using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;

namespace BotLib.Collection
{
    public static class ItemCache
    {
        private static Dictionary<string, Data> _dict = new Dictionary<string, Data>();

        private class Data
        {
            public object Source;
            public DateTime CacheTime;
        }

        public static T GetData<T>(Func<T> producer, out string key, int timeoutMs, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            key = caller + line + path;
            if ( !_dict.ContainsKey(key) || timeoutMs == 0 || (timeoutMs > 0 && _dict[key].CacheTime.xIsTimeElapseMoreThanMs(timeoutMs)))
            {
                _dict[key] = new Data
                {
                    Source = producer(),
                    CacheTime = DateTime.Now
                };
            }
            return (T)((object)_dict[key].Source);
        }

        public static T GetData<T>(Func<T> producer, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            string text;
            return GetData<T>(producer, out text, 0, caller, path, line);
        }

        public static T GetData<T>(Func<T> producer, int timeoutMs, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            string text;
            return GetData<T>(producer, out text, timeoutMs, caller, path, line);
        }

        public static T GetData<T>(Func<T> producer, out string key, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            return GetData<T>(producer, out key, 0, caller, path, line);
        }

        public static void ClearCachedItem(string key)
        {
            _dict.Remove(key);
        }

        public static void ClearCachedItem(List<string> klist)
        {
            foreach (string key in klist)
            {
                _dict.Remove(key);
            }
        }

    }
}
