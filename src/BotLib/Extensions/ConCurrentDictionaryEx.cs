using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class ConCurrentDictionaryEx
    {
        public static bool xTryRemove<KT, VT>(this ConcurrentDictionary<KT, VT> dict, KT key)
        {
            VT vt;
            return dict.TryRemove(key, out vt);
        }

        public static TValue xTryGetValueAndCreateIfNotExist<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> func)
		{
            TValue tvalue = default(TValue);
			if ( (key == null || !dict.TryGetValue(key, out tvalue)) && func != null)
			{
				tvalue = func(key);
				dict[key] = tvalue;
			}
			return tvalue;
		}
    }
}
