using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace BotLib.Wpf.Extensions
{
    public static class ParagraphEx
    {
        public static LineBreak xAppendLineBreak(this Paragraph p)
        {
            LineBreak lineBreak = new LineBreak();
            p.Inlines.Add(lineBreak);
            return lineBreak;
        }

        public static Run xAppendText(this Paragraph p, string text, Brush foreground = null)
        {
            Run run = new Run(text);
            if (foreground != null)
            {
                run.Foreground = foreground;
            }
            p.Inlines.Add(run);
            return run;
        }

        public static Bold xAppendBoldText(this Paragraph p, string text, Brush foreground = null)
        {
            Run childInline = new Run(text);
            Bold bold = new Bold(childInline);
            if (foreground != null)
            {
                bold.Foreground = foreground;
            }
            p.Inlines.Add(bold);
            return bold;
        }
    }
}
