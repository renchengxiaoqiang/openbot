using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using BotLib.Extensions;

namespace BotLib.Wpf.Extensions
{
    public static class RichTextBoxEx
    {
        public static void ReplaceTextToBanJiao(this RichTextBox rtb)
        {
            rtb.TraverseRun(r => 
            {
                string bjText = r.Text.xToBanJiao();
                if (r.Text != bjText)
                {
                    r.Text = bjText;
                }
                return true;
            });
        }

        public static void TraverseRun(this RichTextBox rtb, Func<Run, bool> func)
        {
            var tp = rtb.Document.ContentStart;
            while (tp.CompareTo(rtb.Document.ContentEnd) < 0)
            {
                if (tp.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    var run = tp.Parent as Run;
                    Util.Assert(run != null);
                    if (!func(run))
                    {
                        break;
                    }
                }
                tp = tp.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        public static TextRange GetRangeIncludeCaret(this RichTextBox rtb, int leftLen, int rightLen)
        {
            return rtb.CaretPosition.GetRangeIncludePointer(leftLen, rightLen);
        }

        public static TextRange GetRangeIncludeCaret(this RichTextBox rtb, string pattern)
        {
            return rtb.GetRangeIncludePointer(pattern, rtb.CaretPosition);
        }

        public static TextRange GetRangeIncludePointer(this RichTextBox rtb, string pattern, TextPointer p)
        {
            TextRange range = null;
            string text = rtb.GetText(true);
            int length = rtb.GetTextBeforePointer(p, 0).Length;
            int skipLen = 0;
            Match match = Regex.Match(text, pattern);
            while (match.Success)
            {
                bool isover;
                if (RichTextBoxEx.IsHitTheRange(length, skipLen, match, out isover))
                {
                    range = rtb.GetRange(skipLen + match.Index, match.Length);
                    break;
                }
                if (isover)
                {
                    break;
                }
                skipLen += match.Index + 1;
                text = text.Substring(match.Index + 1);
                match = Regex.Match(text, pattern);
            }
            return range;
        }

        private static bool IsHitTheRange(int caretIndex, int skipLen, Match m, out bool isover)
        {
            int startIdx = m.Index + skipLen;
            int endIdx = startIdx + m.Length;
            isover = (caretIndex >= endIdx);
            return !isover && caretIndex >= startIdx;
        }

        public static TextRange GetRangeBeforeCaret(this RichTextBox tb, int len)
        {
            return tb.GetRange(tb.CaretPosition, len * -1);
        }

        public static TextRange GetRange(this RichTextBox rtb, TextPointer p, int len)
        {
            try
            {
                if (p == null)
                {
                    return null;
                }
                TextPointer positionAtCharOffset;
                if (len >= 0)
                {
                    p = p.GetPositionAtOffset(0, LogicalDirection.Forward);
                    positionAtCharOffset = p.GetPositionAtCharOffset(len, LogicalDirection.Backward);
                    return new TextRange(p, positionAtCharOffset);
                }
                p = p.GetPositionAtOffset(0, LogicalDirection.Backward);
                positionAtCharOffset = p.GetPositionAtCharOffset(len, LogicalDirection.Forward);
                return new TextRange(positionAtCharOffset, p);
            }
            catch
            {
            }
            return null;
        }

        public static TextRange GetRange(this RichTextBox tb, int index, int len)
        {
            TextPointer positionAtCharOffset = tb.Document.ContentStart.GetPositionAtCharOffset(index, LogicalDirection.Forward);
            return tb.GetRange(positionAtCharOffset, len);
        }

        public static TextRange GetRange(this RichTextBox tb, int index)
        {
            TextPointer positionAtCharOffset = tb.Document.ContentStart.GetPositionAtCharOffset(index, LogicalDirection.Forward);
            TextPointer contentEnd = tb.Document.ContentEnd;
            return new TextRange(positionAtCharOffset, contentEnd);
        }

        public static TextRange GetDocumentRange(this RichTextBox rtb)
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        }

        public static string GetText(this RichTextBox rtb, bool ignoreParagraph = true)
        {
            string result;
            if (ignoreParagraph)
            {
                result = rtb.GetTextBeforePointer(rtb.Document.ContentEnd, 0);
            }
            else
            {
                result = rtb.GetDocumentRange().Text;
            }
            return result;
        }

        public static void SetText(this RichTextBox rtb, string txt)
        {
            rtb.GetDocumentRange().Text = (txt ?? "");
        }

        public static string GetTextBeforeCaret(this RichTextBox rtb, int length = 0)
        {
            return rtb.GetTextBeforePointer(rtb.CaretPosition, length);
        }

        public static string GetTextBeforePointer(this RichTextBox rtb, TextPointer p, int length = 0)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (p.CompareTo(rtb.Document.ContentStart) > 0)
            {
                if (p.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text)
                {
                    stringBuilder.Insert(0, p.GetTextInRun(LogicalDirection.Backward));
                    if (length > 0 && stringBuilder.Length > length)
                    {
                        break;
                    }
                }
                p = p.GetNextContextPosition(LogicalDirection.Backward);
            }
            string text = stringBuilder.ToString();
            if (length > 0 && text.Length > length)
            {
                text = text.Substring(text.Length - length);
            }
            return text;
        }

        public static string GetTextAfterCaret(this RichTextBox rtb, int length = 0)
        {
            return rtb.GetTextAfterPointer(rtb.CaretPosition, length);
        }

        public static string GetTextAfterPointer(this RichTextBox rtb, TextPointer p, int length = 0)
        {
            var sb = new StringBuilder();
            while (p.CompareTo(rtb.Document.ContentEnd) < 0)
            {
                if (p.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    sb.Append(p.GetTextInRun(LogicalDirection.Forward));
                    if (length > 0 && sb.Length > length)
                    {
                        break;
                    }
                }
                p = p.GetNextContextPosition(LogicalDirection.Forward);
            }
            string text = sb.ToString();
            if (length > 0 && text.Length > length)
            {
                text = text.Substring(0, length);
            }
            return text;
        }

        public static string GetRichContentXaml(this RichTextBox rtb)
        {
            return rtb.GetDocumentRange().GetRichContentXaml();
        }
    }
}
