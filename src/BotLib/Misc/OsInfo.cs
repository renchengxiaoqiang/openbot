using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Misc
{
    public class OsInfo
    {
        private static bool? _isSimplified = null;

        private static string GetOsInfoFromRegEdit()
        {
            var osinfo = string.Empty;
            try
            {
                using (var regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion"))
                {
                    osinfo = string.Format("操作系统={0},{1},{2}", regKey.GetValue("ProductName") ?? "", regKey.GetValue("EditionID") ?? "", regKey.GetValue("CurrentVersion") ?? "");
                }
                using (var regKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment\\"))
                {
                    osinfo += string.Format("({0}位)", regKey.GetValue("PROCESSOR_ARCHITECTURE"));
                }
            }
            catch (Exception ex)
            {
                osinfo = "无法从“注册表”中获取系统信息，exp=" + ex.Message;
            }
            return osinfo;
        }

        public static string GetOsInfo()
        {
            return GetOsInfoFromRegEdit() + "," + Environment.OSVersion.VersionString + string.Format("(is64bit={0})", Environment.Is64BitOperatingSystem);
        }

        public static bool IsWindows7
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;
            }
        }

        public static bool IsChineseSimplifiedOs()
        {
            if (_isSimplified == null)
            {
                _isSimplified = new bool?(CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName.ToLower() == "chs");
            }
            return _isSimplified.HasValue && _isSimplified.Value;
        }

    }
}
