using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class ListEx
    {
        public static List<T> Create<T>(IEnumerable<T> source)
        {
            var list = new List<T>();
            try
            {
                if (!source.xIsNullOrEmpty())
                {
                    list = new List<T>(source);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return list;
        }

        public static List<T> xGetRange<T>(this List<T> list, int fromIdx)
        {
            if (fromIdx < list.Count)
            {
                list = list.GetRange(fromIdx, list.Count - fromIdx);
            }
            return list;
        }

        public static bool xEquals<T>(this List<T> l1, List<T> l2)
        {
            if (l1 == null || l2 == null) return false;
            if (l1 == l2) return true;
            var rt = false;
            if (l1.Count == l2.Count)
            {
                rt = true;
                for (int i = 0; i < l1.Count; i++)
                {
                    T t = l1[i];
                    if (!t.Equals(l2[i]))
                    {
                        rt = false;
                        break;
                    }
                }
            }

            return rt;
        }

        public static void xRemoveLast<T>(this List<T> list)
        {
            if (!list.xIsNullOrEmpty())
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        public static bool xRemoveLastAndReturn<T>(this List<T> list, out T removed)
        {
            bool rt = false;
            removed = default(T);
            if (!list.xIsNullOrEmpty())
            {
                int index = list.Count - 1;
                removed = list[index];
                list.RemoveAt(index);
                rt = true;
            }
            return rt;
        }

        public static void xRemoveFirstIfNotNull<T>(this List<T> list)
        {
            if (!list.xIsNullOrEmpty())
            {
                list.RemoveAt(0);
            }
        }

        public static string xToCsvStringWithoutEscape(this List<string> slist)
        {
            return slist.xToString(",", false);
        }

        public static List<string> xRemoveNullOrEmptyString(this List<string> slist, bool isTrim = false)
        {
            if (slist == null || slist.Count == 0) return slist;
            return slist.Where(k => !string.IsNullOrEmpty(isTrim ? ((k != null) ? k.Trim() : null) : k)).ToList();
        }

        public static void xSortStringByAsciiAsc(this List<string> self)
        {
            if (self != null)
            {
                self.Sort((l, r) => string.CompareOrdinal(l, r));
            }
        }

        public static void xAddRange<T>(this List<T> self, List<T> other)
        {
            if (self != null && other != null && other.Count > 0)
            {
                self.AddRange(other);
            }
        }

        public static T xRandomSelectOneItem<T>(this List<T> self)
        {
            T t = default(T);
            if (self != null && self.Count != 0)
            {
                int index = RandomEx.Rand.Next(self.Count);
                t = self[index];
            }
            return t;
        }

        public static T xLast<T>(this List<T> self)
        {
            T t = default(T);
            if (self != null && self.Count > 0)
            {
                t = self[self.Count - 1];
            }
            return t;
        }

        public static bool xIsNullOrEmpty<T>(this List<T> self)
        {
            return self == null || self.Count == 0;
        }

        public static bool xNotEmpty<T>(this List<T> self)
        {
            return self != null && self.Count > 0;
        }

        public static int xCount<T>(this List<T> self)
        {
            return (self == null) ? 0 : self.Count;
        }

        public static List<T> xUnion<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null) return list2;
            if (list2 != null && list2.Count > 0)
            {
                list1.AddRange(list2);
            }
            return list1;
        }

        public static List<T> xRemove<T>(this List<T> lst, Predicate<T> pred)
        {
            return lst.Where(k=>pred(k)).ToList();
        }

        public static void xRemoveFromIndex<T>(this List<T> self, int fromIndex)
        {
            if (self != null)
            {
                if (self.Count() > fromIndex)
                {
                    self.RemoveRange(fromIndex, self.Count<T>() - fromIndex);
                }
            }
        }

        public static bool xReplaceFirstMatch<T>(this List<T> self, T entityNew, Predicate<T> pred, bool addIfNotReplaced = false)
        {
            bool replaced = false;
            int idx = self.FindIndex(pred);
            if (idx >= 0)
            {
                self[idx] = entityNew;
                replaced = true;
            }
            if (!replaced && addIfNotReplaced)
            {
                self.Add(entityNew);
            }
            return replaced;
        }

        public static ConcurrentBag<T> xReplaceMatch<T>(this ConcurrentBag<T> bag, T entityNew, Predicate<T> pred, bool addIfNotReplaced = false)
        {
            ConcurrentBag<T> concurrentBag = bag;
            bool replaced = false;
            if (bag.xContains(pred))
            {
                replaced = true;
                foreach (T t in bag)
                {
                    if (pred(t))
                    {
                        concurrentBag.Add(entityNew);
                    }
                    else
                    {
                        concurrentBag.Add(t);
                    }
                }
            }
            if (!replaced && addIfNotReplaced)
            {
                concurrentBag.Add(entityNew);
            }
            return concurrentBag;
        }

        public static bool xContains<T>(this ConcurrentBag<T> self, Predicate<T> pred)
        {
            return self.Any(k => pred(k));
        }

        public static bool xReplaceFirst<T>(this List<T> self, T old, T enew)
        {
            bool rt = false;
            int idx = self.IndexOf(old);
            if (idx >= 0)
            {
                self[idx] = enew;
                rt = true;
            }
            return rt;
        }

        public static List<T> xCopy<T>(this List<T> self, int fromidx, int length = -1)
        {
            var list = new List<T>();
            int cnt = (length >= 0) ? (length + fromidx) : self.Count;
            if ( cnt <= fromidx)
            {
                cnt = fromidx + 1;
            }
            if (cnt > self.Count)
            {
                cnt = self.Count;
            }
            for (int i = fromidx; i < cnt; i++)
            {
                list.Add(self[i]);
            }
            return list;
        }

        public static List<T> xClone<T>(this List<T> list)
        {
            return list.xCopy(0, -1);
        }
    }
}
