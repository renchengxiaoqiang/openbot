using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation
{
    public class HwndInfo
    {
        public string Description { get; set; }
        public int Handle { get; set; }
        
        public HwndInfo(int handle, string description)
        {
            Handle = handle;
            Description = description;
        }
    }
}
