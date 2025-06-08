using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class IEnumerableEx
    {
        public static T xRandom<T>(this IEnumerable<T> source)
        {
            T t = default(T);
            if (source != null)
            {
                int cnt = source.Count<T>();
                if (cnt > 0)
                {
                    int index = RandomEx.Rand.Next(cnt);
                    t = source.ElementAt(index);
                }
            }
            return t;
        }

        public static IEnumerable<T> xSafeForEach<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static string xToString<T>(this IEnumerable<T> list, string seperator = ",", bool printNull = true)
        {
            if (list == null) return string.Empty;

            var sb = new StringBuilder();
            foreach (T t in list)
            {
                string text = (t != null) ? t.ToString() : null;
                sb.Append(text ?? (printNull ? "null" : ""));
                sb.Append(seperator);
            }
            int length = seperator.Length;
            if (sb.Length >= length)
            {
                sb.Remove(sb.Length - length, length);
            }
            return sb.ToString();
        }

        public static bool xIsNullOrEmpty<T>(this IEnumerable<T> input)
        {
            return input == null || !input.Any<T>();
        }
    }
}
