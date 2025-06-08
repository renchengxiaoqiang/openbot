using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace BotLib
{
    public class Params
    {
        private static bool? _isServerLib;
        private static bool? _isDevoloperClient;

        public static bool IsServerLib
        {
            get
            {
                if (!_isServerLib.HasValue)
                {
                    _isServerLib = HostingEnvironment.IsHosted;
                }
                return _isServerLib.Value;
            }
        }

        public static bool IsDevoloperClient
        {
            get
            {
                if (!_isDevoloperClient.HasValue)
                {
                    _isDevoloperClient = File.Exists("DevoloperClientMark.txt");
                }
                return _isDevoloperClient.Value;
            }
        }
    }
}
