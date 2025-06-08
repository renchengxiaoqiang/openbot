using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BotLib.Extensions;

namespace BotLib.Wpf.Extensions
{
    public static class ItemCollectionEx
    {
        public static void xAddRange<T>(this ItemCollection coll, IEnumerable<T> list)
        {
            foreach (T t in list.xSafeForEach<T>())
            {
                coll.Add(t);
            }
        }

        public static void xRemoveRange(this ItemCollection coll, int idx, int count = -1)
        {
            if (count < 0)
            {
                count = coll.Count - idx;
            }
            for (int i = 0; i < count; i++)
            {
                coll.RemoveAt(idx);
            }
        }
    }
}
