using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BotLib.Wpf.Extensions
{
    public static class TextBoxEx
    {
        public static void xMoveCaretToTail(this TextBox tbox)
        {
            if (tbox != null)
            {
                tbox.CaretIndex = tbox.Text.Length;
                tbox.ScrollToEnd();
            }
        }

        public static void xInsertOrAppend(this TextBox tbox, string txt)
        {
            if (string.IsNullOrEmpty(txt)) return;

            int startIdx = tbox.SelectionStart;
            string text = string.Empty;
            if (tbox.SelectionStart >= 0 && tbox.SelectionStart < tbox.Text.Length)
            {
                text = tbox.Text.Substring(0, startIdx) + txt + tbox.Text.Substring(startIdx + tbox.SelectionLength);
                startIdx += txt.Length;
            }
            else
            {
                text = tbox.Text + txt;
                startIdx = text.Length;
            }
            tbox.Text = (text ?? "");
            tbox.SelectionStart = startIdx;
            tbox.SelectionLength = 0;
            tbox.Focus();

        }
    }
}
