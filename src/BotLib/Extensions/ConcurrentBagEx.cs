using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class ConcurrentBagEx
    {
        public static ConcurrentBag<T> xRemove<T>(this ConcurrentBag<T> input, T obj)
        {
            if (input.xIsNullOrEmpty() || obj == null) return input;

            ConcurrentBag<T> concurrentBag = new ConcurrentBag<T>();
            foreach (T item in input)
            {
                if (!item.Equals(obj))
                {
                    concurrentBag.Add(item);
                }
            }
            return concurrentBag;
        }

        public static ConcurrentBag<T> xRemove<T>(this ConcurrentBag<T> input, IEnumerable<T> rems)
        {
            if (rems.xIsNullOrEmpty() || input.xIsNullOrEmpty()) return input;

            ConcurrentBag<T> concurrentBag = new ConcurrentBag<T>();
            foreach (T t in input)
            {
                if (!rems.Contains(t))
                {
                    concurrentBag.Add(t);
                }
            }
            return concurrentBag;
        }

        public static ConcurrentBag<T> xRemoveAll<T>(this ConcurrentBag<T> input, Func<T, bool> predict)
        {
            return new ConcurrentBag<T>(input.Where(k => !predict(k)).ToList());
        }
    }
}
