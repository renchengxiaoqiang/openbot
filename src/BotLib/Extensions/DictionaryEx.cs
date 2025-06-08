using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class DictionaryEx
    {
        public static bool xEquals<TKey, TValue>(this Dictionary<TKey, TValue> d1, Dictionary<TKey, TValue> d2)
        {
            d1 = (d1 ?? new Dictionary<TKey, TValue>());
            d2 = (d2 ?? new Dictionary<TKey, TValue>());
            if (d1.Count != d2.Count) return false;

            foreach (var kv in d1)
            {
                bool notEqual;
                if (d2.ContainsKey(kv.Key))
                {
                    TValue tv = d2[kv.Key];
                    notEqual = !tv.Equals(kv.Value);
                }
                else
                {
                    notEqual = true;
                }
                if (notEqual)
                {
                    return false;
                }
            }
            return true;
        }

        public static int xRemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dict, Predicate<KeyValuePair<TKey, TValue>> pred)
        {
            int delCnt = 0;
            if (dict != null && dict.Count > 0)
            {
                try
                {
                    var keys = (from kv in dict
                                       where pred(kv)
                                       select kv into grp
                                       select grp.Key).ToList<TKey>();
                    if (!keys.xIsNullOrEmpty<TKey>())
                    {
                        foreach (TKey key in keys)
                        {
                            if (dict.Remove(key))
                            {
                                delCnt++;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
            return delCnt;
        }

        public static Dictionary<TKey, TValue> xRemoveNullValue<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            var dictNotNull = new Dictionary<TKey, TValue>();
            foreach (var kv in dict)
            {
                if (kv.Value != null)
                {
                    dictNotNull.Add(kv.Key, kv.Value);
                }
            }
            return dictNotNull;
        }

        public static void xAddOrUpdateMany<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> d2)
        {
            if (d2 != null && dict != null)
            {
                foreach (var kv in d2)
                {
                    dict[kv.Key] = kv.Value;
                }
            }
        }

        public static TValue xTryGetValueAndCreateIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> func)
        {
            var val = default(TValue);
            if ((key == null || !dict.TryGetValue(key, out val)) && func != null)
            {
                val = func(key);
                dict[key] = val;
            }
            return val;
        }

        public static TValue xTryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue def = default(TValue))
        {
            TValue val = def;
            if (!dict.TryGetValue(key, out val))
            {
                val = def;
            }
            return val;
        }
    }
}
