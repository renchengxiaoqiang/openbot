using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators
{
    public class QnChatWnd
    {
        public readonly int Hwnd;
        public readonly string Name;
        public readonly int Pid;
        public QnChatWnd(string name, int hWnd, int pid)
		{
			Name = name;
            Hwnd = hWnd;
            Pid = pid;
		}

    }
}
