using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using BotLib.Extensions;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;

namespace BotLib.Wpf.Extensions
{
    public static class TextBlockEx
    {
        public static void xAppendLinksToTextBlock(this TextBlock tb, params Hyperlink[] args)
        {
            tb.xAppendText("（", null, null);
            for (int i = 0; i < args.Length; i++)
            {
                tb.Inlines.Add(args[i]);
                if ( i < args.Length - 1)
                {
                    tb.xAppendText("，", null, null);
                }
            }
            tb.xAppendText("）", null, null);
        }

        public static string xGetText(this TextBlock tb)
        {
            string text = "";
            if (tb != null)
            {
                if (tb.Inlines.xIsNullOrEmpty())
                {
                    text = tb.Text;
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (Inline line in tb.Inlines)
                    {
                        sb.Append(line.xGetText());
                    }
                    text = sb.ToString();
                }
            }
            return text;
        }

        public static Run xAppendText(this TextBlock tb, string txt, Brush foreground = null, Brush background = null)
        {
            Run run = new Run(txt);
            if (foreground != null)
            {
                run.Foreground = foreground;
            }
            if (background != null)
            {
                run.Background = background;
            }
            tb.Inlines.Add(run);
            return run;
        }

        public static Hyperlink xAppendHyperlink(this TextBlock tb, string txt, bool focusable = false)
        {
            Hyperlink hyperlink = new Hyperlink();
            hyperlink.Focusable = focusable;
            hyperlink.Inlines.Add(txt);
            tb.Inlines.Add(hyperlink);
            return hyperlink;
        }

        public static void xCopyToClipboardOnRightClick(this TextBlock tblk, string txt, bool clearPreTip = false)
        {
            string text = "右击，复制到剪贴板";
            if (tblk.ToolTip == null || clearPreTip)
            {
                tblk.ToolTip = text;
            }
            else
            {
                if (tblk.ToolTip is string)
                {
                    tblk.ToolTip = string.Format("{0}\r\n\r\n{1}", tblk.ToolTip as string, text);
                }
            }
            tblk.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    Clipboard.SetDataObject(txt);
                }
            };
        }

        public static TextBlock Create(string str, params object[] args)
        {
            TextBlock tb = new TextBlock();
            if (args.Length != 0)
            {
                str = string.Format(str, args);
            }
            tb.Text = str;
            return tb;
        }

        public static TextBlock CreateWithColor(string str, Brush foreground = null, Brush background = null)
        {
            TextBlock textBlock = new TextBlock();
            Run run = textBlock.xAppendText(str, foreground, background);
            return textBlock;
        }

        public static TextBlock CreateLightGray(string format, params object[] args)
        {
            return TextBlockEx.CreateWithColor(string.Format(format, args), Brushes.LightGray, null);
        }
    }
}
