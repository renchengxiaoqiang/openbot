using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace BotLib.Wpf.Extensions
{
    public static class TextPointerEx
    {
        public static TextRange GetRangeIncludePointer(this TextPointer p, int leftLen, int rightLen)
        {
            TextPointer positionAtCharOffset = p.GetPositionAtCharOffset(-1 * leftLen, LogicalDirection.Forward);
            TextPointer positionAtCharOffset2 = p.GetPositionAtCharOffset(rightLen, LogicalDirection.Backward);
            return new TextRange(positionAtCharOffset, positionAtCharOffset2);
        }

        public static TextPointer GetPositionAtCharOffset(this TextPointer p, int offset, LogicalDirection returnDirection = LogicalDirection.Forward)
        {
            var tp = p;
            if (offset == 0)
            {
                tp = tp.GetPositionAtOffset(0, returnDirection);
                return tp;
            }

            var logicalDirection = (offset > 0) ? LogicalDirection.Forward : LogicalDirection.Backward;
            if (offset < 0)
            {
                offset *= -1;
            }
            if (tp.LogicalDirection != logicalDirection)
            {
                tp = tp.GetPositionAtOffset(0, logicalDirection);
            }
            int len = 0;
            while (tp != null)
            {
                if (tp.GetPointerContext(logicalDirection) == TextPointerContext.Text)
                {
                    string textInRun = tp.GetTextInRun(logicalDirection);
                    if (textInRun.Length + len >= offset)
                    {
                        int offset2 = (logicalDirection == LogicalDirection.Forward) ? (offset - len) : (len - offset);
                        tp = tp.GetPositionAtOffset(offset2, returnDirection);
                        break;
                    }
                    len += textInRun.Length;
                }
                tp = tp.GetNextContextPosition(logicalDirection);
            }

            return tp;
        }
    }
}
