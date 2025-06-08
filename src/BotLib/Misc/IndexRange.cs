using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class IndexRange
    {
        public readonly int Start;
        public readonly int Length;
        public readonly int NextStart;
        public IndexRange(int fromidx, int length)
        {
            Util.Assert(fromidx >= 0 && length >= 0);
            this.Start = fromidx;
            this.Length = length;
            this.NextStart = fromidx + length;
        }

        public bool IsInsideRange(int idx)
        {
            return idx >= this.Start && idx < this.NextStart;
        }

        public static IntersectTypeEnum GetIntersectType(int r1Start, int r1NextStart, int r2Start, int r2NextStart)
        {
            var intersectType = IntersectTypeEnum.NotIntersect;
            if (r1Start == r2Start && r1NextStart == r2NextStart) 
            {
                intersectType = IntersectTypeEnum.R1EqualR2;
            }
            else
            {
                if (r1Start >= r2Start && r1Start < r2NextStart)
                {
                    if (r1NextStart - 1 >= r2Start && r1NextStart - 1 < r2NextStart)
                    {
                        intersectType = IntersectTypeEnum.R2IncludeR1;
                    }
                    else
                    {
                        intersectType = IntersectTypeEnum.R1LeftPartIntersectR2RightPart;
                    }
                }
                else
                {
                    if (r1NextStart - 1 >= r2Start && r1NextStart - 1 < r2NextStart)
                    {
                        intersectType = IntersectTypeEnum.R1RightPartIntersectR2LeftPart;
                    }
                    else
                    {
                        if (r2Start >= r1Start && r2Start < r1NextStart)
                        {
                            if (r2NextStart - 1 >= r1Start && r2NextStart - 1 < r1NextStart)
                            {
                                intersectType = IntersectTypeEnum.R1IncludeR2;
                            }
                        }
                    }
                }
            }
            return intersectType;
        }

        public static IndexRange.IntersectTypeEnum GetIntersectType(IndexRange r1, IndexRange r2)
        {
            return IndexRange.GetIntersectType(r1.Start, r1.NextStart, r2.Start, r2.NextStart);
        }

        public IndexRange.IntersectTypeEnum GetIntersectType(IndexRange r2)
        {
            return IndexRange.GetIntersectType(this.Start, this.NextStart, r2.Start, r2.NextStart);
        }

        public static bool IsIndexInsideRanges(int i, List<IndexRange> ranges)
        {
            foreach (IndexRange indexRange in ranges)
            {
                if (indexRange.IsInsideRange(i))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<IndexRange> GetExceptRanges(List<IndexRange> ranges, int startIndex, int nextStartIndex)
        {
            List<IndexRange> list = new List<IndexRange>();
            foreach (IndexRange indexRange in ranges)
            {
                if (indexRange.Start > startIndex)
                {
                    list.Add(new IndexRange(startIndex, indexRange.Start - startIndex));
                }
                startIndex = indexRange.NextStart;
            }
            if (startIndex < nextStartIndex)
            {
                list.Add(new IndexRange(startIndex, nextStartIndex - startIndex));
            }
            return list;
        }

        public static void SortRanges(List<IndexRange> irlist)
        {
            if (irlist != null)
            {
                irlist.Sort((l, r) => l.Start.CompareTo(r.Start));
            }
        }

        public static List<IndexRange> GetExceptRanges(List<IndexRange> ranges, string txt)
        {
            return IndexRange.GetExceptRanges(ranges, 0, txt.Length);
        }

        public static void SortAndMergeRanges(List<IndexRange> ranges)
        {
            IndexRange.SortRanges(ranges);
            for (int i = 1; i < ranges.Count; i++)
            {
                if (ranges[i].Start <= ranges[i - 1].NextStart)
                {
                    int idx = Math.Max(ranges[i - 1].NextStart, ranges[i].NextStart);
                    ranges[i - 1] = new IndexRange(ranges[i - 1].Start, idx - ranges[i - 1].Start);
                    ranges.RemoveAt(i);
                    i--;
                }
            }
        }


        public enum IntersectTypeEnum
        {
            NotIntersect,
            R1IncludeR2,
            R2IncludeR1,
            R1EqualR2,
            R1RightPartIntersectR2LeftPart,
            R1LeftPartIntersectR2RightPart
        }
    }
}
