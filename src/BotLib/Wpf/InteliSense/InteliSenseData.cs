using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Wpf.InteliSense
{
    public class InteliSenseData
    {
        public object Text { get; set; }
        public object Tag { get; set; }
        public string ToolTip { get; set; }
        public string InteliSenseFor { get; set; }
    }
}
