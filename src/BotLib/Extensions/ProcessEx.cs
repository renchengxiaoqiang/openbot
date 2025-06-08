using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
	public static class ProcessEx
	{
		public static void KillProcess(string pname, string cfn)
		{
			try
			{
				foreach (var process in Process.GetProcessesByName(pname))
				{
					var mainModuleFilepathByPid = GetMainModuleFilepathByPid(process.Id);
					if (cfn == mainModuleFilepathByPid)
					{
						process.Kill();
					}
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		public static string GetMainModuleFilepathByPid(int processId)
		{
			var queryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
			var managementObjectSearcher = new ManagementObjectSearcher(queryString);
			var managementObjectCollection = managementObjectSearcher.Get();
			var managementObject = managementObjectCollection.Cast<ManagementObject>().FirstOrDefault();
			managementObjectSearcher.Dispose();
			managementObject.Dispose();
			if (managementObject == null) return null;
			return managementObject["ExecutablePath"].ToString();
		}

		public static Process Run(string cfn, string param = null)
		{
			var process = new Process();
			process.StartInfo.FileName = cfn;
			process.StartInfo.Arguments = param;
			process.Start();
			return process;
		}

		private static string FindIndexedProcessName(int pid)
		{
			var processName = Process.GetProcessById(pid).ProcessName;
			var processesByName = Process.GetProcessesByName(processName);
			var indexOfProcessName = string.Empty;
			for (int i = 0; i < processesByName.Length; i++)
			{
				indexOfProcessName = ((i == 0) ? processName : (processName + "#" + i));
				var performanceCounter = new PerformanceCounter("Process", "ID Process", indexOfProcessName);
				if (performanceCounter.NextValue() == pid)
				{
					break;
				}
			}
			return indexOfProcessName;
		}

		private static Process FindPidFromIndexedProcessName(string indexedProcessName)
		{
			var performanceCounter = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
			return Process.GetProcessById((int)performanceCounter.NextValue());
		}

		public static Process Parent(this Process process)
		{
			return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
		}
	}
}
