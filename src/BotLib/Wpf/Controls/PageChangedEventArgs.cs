using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Wpf.Controls
{
    public class PageChangedEventArgs : EventArgs
    {
        public int NewPageNo { get; private set; }
        public int OldPageNo { get; private set; }

        public PageChangedEventArgs(int newNo, int oldNo)
        {
            this.NewPageNo = newNo;
            this.OldPageNo = oldNo;
        }
    }
}
