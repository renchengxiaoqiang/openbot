using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BotLib.Extensions;

namespace BotLib.Misc
{
    public static class ComputerInfo
    {
        private static string _cpuName;
        private static int _logicalScreenHeight = -1;
        private static int _logicalScreenWidth = -1;
        private static int _physicalScreenHeight = -1;

        public static double PixelMagnify = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;
        private static int _physicalScreenWidth = -1;
        private static double _xRatioOfPhysicalVsLogical = 0.0;
        private static double _yRatioOfPhysicalVsLogical = 0.0;
        private static double _xRatioOfLogicalVsPhysical = 0.0;

        private static double _yRatioOfLogicalVsPhysical = 0.0;

        public static string CpuName
        {
            get
            {
                if (_cpuName == null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                    foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
                    {
                        ManagementObject managementObject = (ManagementObject)managementBaseObject;
                        stringBuilder.Append(managementObject["Name"].ToString() + ",");
                    }
                    _cpuName = stringBuilder.ToString().xRemoveLastChar();
                }
                return _cpuName;
            }
        }

        public static int LogicalScreenHeight
        {
            get
            {
                if (_logicalScreenHeight < 0)
                {
                    InitScreenInfoIfNeed();
                }
                return _logicalScreenHeight;
            }
        }

        public static int LogicalScreenWidth
        {
            get
            {
                if (_logicalScreenWidth < 0)
                {
                    InitScreenInfoIfNeed();
                }
                return _logicalScreenWidth;
            }
        }

        public static int PhysicalScreenHeight
        {
            get
            {
                if (_physicalScreenHeight < 0)
                {
                    InitScreenInfoIfNeed();
                }
                return _physicalScreenHeight;
            }
        }

        public static double xMagnify = (double)PhysicalScreenWidth / (double)LogicalScreenWidth;
        public static double XMagnify
        {
            get
            {
                return xMagnify;
            }
            private set
            {
                xMagnify = value;
            }
        }
        public static double yMagnify = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;
        public static double YMagnify
        {
            get
            {
                return yMagnify;
            }
            private set
            {
                yMagnify = value;
            }
        }
        public static double xMagnifyReciprocal = 1.0 / XMagnify;
        public static double XMagnifyReciprocal
        {
            get
            {
                return xMagnifyReciprocal;
            }
            private set
            {
                xMagnifyReciprocal = value;
            }
        }
        public static double yMagnifyReciprocal = 1.0 / YMagnify;
        public static double YMagnifyReciprocal
        {
            get
            {
                return yMagnifyReciprocal;
            }
            private set
            {
                yMagnifyReciprocal = value;
            }
        }

        public static int PhysicalScreenWidth
        {
            get
            {
                if (_physicalScreenWidth < 0)
                {
                    InitScreenInfoIfNeed();
                }
                return _physicalScreenWidth;
            }
        }

        public static int CalcWidthWithXMagnify(double width)
        {
            return (int)Math.Round(width * ComputerInfo.XMagnify);
        }

        public static int CalcHeightWithYMagnify(double height)
        {
            return (int)Math.Round(height * ComputerInfo.YMagnify);
        }

        public static void InitScreenInfoIfNeed()
        {
            if (_xRatioOfLogicalVsPhysical <= 0.0)
            {
                Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
                IntPtr hdc = graphics.GetHdc();
                _logicalScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
                _physicalScreenHeight = GetDeviceCaps(hdc, 117);
                _yRatioOfLogicalVsPhysical = (double)_logicalScreenHeight / (double)_physicalScreenHeight;
                _yRatioOfPhysicalVsLogical = (double)_physicalScreenHeight / (double)_logicalScreenHeight;
                _logicalScreenWidth = (int)SystemParameters.PrimaryScreenWidth;
                _physicalScreenWidth = GetDeviceCaps(hdc, 118);
                _xRatioOfLogicalVsPhysical = (double)_logicalScreenWidth / (double)_physicalScreenWidth;
                _xRatioOfPhysicalVsLogical = (double)_physicalScreenWidth / (double)_logicalScreenWidth;
            }
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public static string GetCpuID()
        {
            try
            {
                //获取CPU序列号代码
                string cpuInfo = "";//cpu序列号
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }
                moc = null;
                mc = null;
                return cpuInfo;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }


        public static string GetMacAddress()
        {
            try
            {
                //获取网卡硬件地址
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        public static string GetIPAddress()
        {
            try
            {
                //获取IP地址
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        //st=mo["IpAddress"].ToString();
                        System.Array ar;
                        ar = (System.Array)(mo.Properties["IpAddress"].Value);
                        st = ar.GetValue(0).ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        public static string GetDiskID()
        {
            try
            {
                //获取硬盘ID
                string HDid = "";
                ManagementClass mc = new ManagementClass("Win32_DiskDrive");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    HDid = (string)mo.Properties["Model"].Value;
                }
                moc = null;
                mc = null;
                return HDid;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        public static string GetUserName()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["UserName"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        public static string GetSystemType()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["SystemType"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }

        public static string GetTotalPhysicalMemory()
        {
            try
            {

                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {

                    st = mo["TotalPhysicalMemory"].ToString();

                }
                moc = null;
                mc = null;
                return st;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }

        public static double GetTotalPhysicalMemoryGByte()
        {
            return Convert.ToDouble(GetTotalPhysicalMemory()) / Math.Pow(2.0, 30.0);
        }

        public static string SysInfoForLog
        {
            get
            {
                var sysinfo = string.Empty;
                try
                {
                    sysinfo = OsInfo.GetOsInfo() + ",内存=" + GetTotalPhysicalMemoryGByte().ToString("F1") + " GB";
                    sysinfo = sysinfo + ",CPU=" + CpuName;
                    sysinfo += string.Format(",物理分辨率={0}x{1},逻辑分辨率={2}x{3}", new object[]
					{
						PhysicalScreenWidth,
						PhysicalScreenHeight,
						LogicalScreenWidth,
						LogicalScreenHeight
					});
                }
                catch (Exception ex)
                {
                    sysinfo = "获取系统信息出错，exp=" + ex.Message;
                }
                return sysinfo;
            }
        }

        public static string GetComputerName()
        {
            try
            {
                return System.Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }
        }


        public enum DeviceCap
        {
            HORZRES = 8,
            VERTRES = 10,
            DESKTOPVERTRES = 117,
            DESKTOPHORZRES
        }
    }
}
