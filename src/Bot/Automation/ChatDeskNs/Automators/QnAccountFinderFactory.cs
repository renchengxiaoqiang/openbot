using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators
{
    public static class QnAccountFinderFactory
    {
        private static QnAccountFinder _finder;
        public static QnAccountFinder Finder
        {
            get
            {
                if (_finder == null)
                {
                    _finder = new QnAccountFinder();
                }
                return _finder;
            }
        }
    }
}
