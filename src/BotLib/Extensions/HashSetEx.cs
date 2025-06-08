using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class HashSetEx
    {
        public static HashSet<T> Create<T>(IEnumerable<T> source)
        {
            var hs = new HashSet<T>();
            try
            {
                if (!source.xIsNullOrEmpty())
                {
                    hs = new HashSet<T>(source);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return hs;
        }

        public static bool xIsNullOrEmpty<T>(this HashSet<T> hs)
        {
            return hs == null || hs.Count == 0;
        }

        public static int xAddRange<T>(this HashSet<T> hs, IEnumerable<T> items)
        {
            int cnt = 0;
            if (items == null) return cnt;
            foreach (T item in items)
            {
                if (hs.Add(item))
                {
                    cnt++;
                }
            }
            return cnt;
        }

        public static int xRemoveRange<T>(this HashSet<T> hs, IEnumerable<T> items)
        {
            int cnt = 0;
            foreach (T item in items.xSafeForEach<T>())
            {
                if (hs.Remove(item))
                {
                    cnt++;
                }
            }
            return cnt;
        }
    }
}
