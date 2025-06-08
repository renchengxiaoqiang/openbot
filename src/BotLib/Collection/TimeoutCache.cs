using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Collection
{
    public class TimeoutCache
    {
        private static ConcurrentDictionary<string, CacheItem> _dict = new ConcurrentDictionary<string, CacheItem>();

        private class CacheItem
        {
            public object Data;
            private int Ms;
            private DateTime Create;

            public CacheItem(object data, int ms)
            {
                Data = data;
                Ms = ms;
                Create = DateTime.Now;
            }

            public bool IsTimeout()
            {
                return (DateTime.Now - Create).TotalMilliseconds > (double)Ms;
            }

        }

        public static bool GetData<T>(out T data, string key)
        {
            data = default(T);
            if (string.IsNullOrEmpty(key)) return false;

            CacheItem cacheItem;
            if (_dict.TryGetValue(key, out cacheItem))
            {
                if (!cacheItem.IsTimeout())
                {
                    data = (T)((object)cacheItem.Data);
                    return true;
                }
            }
            return false;
        }

        
        public static string SetData(object data, int timeoutMs, [System.Runtime.CompilerServices.CallerFilePath] string fn = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0, [System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            string key = GetKey(fn, line, name);
            _dict[key] = new CacheItem(data, timeoutMs);
            return key;
        }

        private static string GetKey(string fn, int line, string name)
        {
            return string.Format("{2},{1},{0}", fn, line, name);
        }

    }
}
