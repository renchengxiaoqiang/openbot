using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using BotLib.Extensions;
using BotLib.Misc;

namespace BotLib.Wpf.Extensions
{
    public static class InlineEx
    {
        public static List<Inline> ConvertTextToInlineConsiderUrl(string txt)
        {
            txt = (txt ?? "");
            List<Inline> inlines = new List<Inline>();
            List<IndexRange> urlRanges = txt.xGetUrlIndexRanges();
            if (urlRanges.xIsNullOrEmpty())
            {
                inlines.Add(new Run(txt));
                return inlines;
            }
            int idx = 0;
            foreach (var urlIr in urlRanges)
            {
                if (urlIr.Start > idx)
                {
                    inlines.Add(new Run(txt.Substring(idx, urlIr.Start - idx)));
                }
                var hyperlink = new Hyperlink();
                string url = txt.Substring(urlIr.Start, urlIr.Length);
                hyperlink.Inlines.Add(new Run(url));
                hyperlink.Click += (sender,e)=>
                {
                    Util.Nav(url);
                };
                inlines.Add(hyperlink);
                idx = urlIr.NextStart;
            }

            return inlines;
        }

        public static string xGetText(this Inline line)
        {
            if (line == null)
                return string.Empty;

            if (line is Run)
            {
                return (line as Run).Text;
            }
            if (line is LineBreak)
            {
                return Environment.NewLine;
            }

            if (line is Span)
            {
                var sb = new StringBuilder();
                Span span = line as Span;
                foreach (var inl in span.Inlines)
                {
                    sb.Append(inl.xGetText());
                }
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
