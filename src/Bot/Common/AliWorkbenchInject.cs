using IniParser.Model;
using IniParser;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using BotLib;
using System.Windows;

namespace Bot.Common
{
    public class AliWorkbenchInject
    {
        private const string AppName = "千牛工作台";
        private const string ExeName = "AliWorkbench.exe";
        private const string ShortcutName = "千牛工作台.lnk";
        private const string WebuiResDir = "newWebui";
        private const string WebuiFile = "webui.zip";
        private const string SignFile = "sign.json";
        private const string ChatRecentHtmlFile = @"web_chat-packer/recent.html";
        private const string ImSupportUrl = @"https://iseiya.taobao.com/imsupport";
        private const string OverWriteUrl = "https://worklink.oss-cn-hangzhou.aliyuncs.com/5CFB5E11D17E63CDD8CB37B52FA6ACFD.js"; 


        public static WorkbenchResult ProcessWorkbench()
        {
            try
            {
                string installPath = FindInstallPath();
                if (string.IsNullOrEmpty(installPath))
                {
                    return new WorkbenchResult { Status = WorkbenchStatus.NotInstalled };
                }

                var resourcePath = FindResourcePath(installPath);
                if (string.IsNullOrEmpty(resourcePath))
                {
                    return new WorkbenchResult { Status = WorkbenchStatus.ResourceNotFound };
                }

                if (IsWorkbenchRunning())
                {
                    return new WorkbenchResult { Status = WorkbenchStatus.Running };
                }


                InjectScript(resourcePath);

                return new WorkbenchResult { Status = WorkbenchStatus.Success };
            }
            catch (Exception ex)
            {
                return new WorkbenchResult { Status = WorkbenchStatus.Failed, Message = ex.Message };
            }
        }

        private static string FindInstallPath()
        {
            string installPath = null;
            try
            {
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("aliim");
                registryKey = registryKey.OpenSubKey("Shell");
                registryKey = registryKey.OpenSubKey("Open");
                registryKey = registryKey.OpenSubKey("Command");
                installPath = registryKey.GetValue("").ToString();
                int idx = installPath.IndexOf("wwcmd.exe");
                installPath = installPath.Substring(1, idx + 8);
                installPath = Directory.GetParent(installPath).Parent.FullName;
            }
            catch (Exception e)
            {
                Log.Exception(e, "installPath");
            }
            return installPath;
        }

        private static string FindResourcePath(string installPath)
        {
            var aliWorkbenchConfigPath = Path.Combine(installPath, "AliWorkbench.ini");
            if (File.Exists(aliWorkbenchConfigPath))
            {
                var version = ReadIniFile(aliWorkbenchConfigPath);
                return Path.Combine(installPath, version, "Resources");
            }
            return string.Empty;
        }


        private static bool IsWorkbenchRunning()
        {
            var processes = Process.GetProcessesByName(ExeName.Replace(".exe", ""));
            return processes.Length > 0;
        }

        private static void InjectScript(string resourcePath)
        {
            var webuiResPath = Path.Combine(resourcePath, WebuiResDir,WebuiFile);
            var signPath = Path.Combine(resourcePath, WebuiResDir, SignFile);

            using (var zipFile = new ZipFile(webuiResPath))
            {
                var entry = zipFile.GetEntry(ChatRecentHtmlFile);
                using (var inputStream = zipFile.GetInputStream(entry))
                {
                    using (var streamReader = new StreamReader(inputStream))
                    {
                        var chatRecentHtmlContent = streamReader.ReadToEnd();
                        if (!chatRecentHtmlContent.Contains(ImSupportUrl)) return;
                        chatRecentHtmlContent = chatRecentHtmlContent.Replace(ImSupportUrl, OverWriteUrl);
                        zipFile.BeginUpdate();
                        zipFile.Add(new ZipStaticDataSource(chatRecentHtmlContent), ChatRecentHtmlFile);
                        zipFile.CommitUpdate();
                        if (File.Exists(signPath))
                        {
                            var signFi = new FileInfo(signPath);
                            if (signFi.Length > 0)
                            {
                                signFi.Delete();
                                File.Create(signPath);
                            }
                        }
                    }
                }
            }

        }

        private static string ReadIniFile(string path)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(path);
            string version = data["Common"]["Version"];
            return version;
        }
    }

    public class WorkbenchResult
    {
        public WorkbenchStatus Status { get; set; }
        public string Message { get; set; }
    }

    public enum WorkbenchStatus
    {
        NotInstalled,
        ResourceNotFound,
        Running,
        Success,
        Failed
    }

    public class ZipStaticDataSource : IStaticDataSource
    {

        private string _content;
        public ZipStaticDataSource(string content)
        {
            _content = content;
        }

        public Stream GetSource()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(_content);
            return new MemoryStream(bytes);
        }
    }
}
