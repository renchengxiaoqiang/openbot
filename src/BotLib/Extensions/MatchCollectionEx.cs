using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class MatchCollectionEx
    {
        public static List<IndexRange> xConvertToIndexRanges(this MatchCollection coll)
        {
            var ranges = new List<IndexRange>();
            if (coll != null && coll.Count > 0)
            {
                foreach (Match match in coll)
                {
                    var range = new IndexRange(match.Index, match.Length);
                    ranges.Add(range);
                }
            }
            return ranges;
        }
    }
}
