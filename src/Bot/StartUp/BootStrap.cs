using BotLib;
using BotLib.Extensions;
using Bot.ControllerNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Common.Db;
using Bot.Common;
using BotLib.Wpf.Extensions;
using Bot.ChromeNs;

namespace Bot
{
    public class BootStrap
    {
        public static void Init()
        {
            ClearTmpPathFiles();
            DeskScanner.LoopScan();
            MyWebSocketServer.WSocketSvrInst.Start();
            QNInject.StartInject();

            //var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"inject.js"));
            //IseiyaHttpProxy.StartProxy(script);
        }

        private static void ClearTmpPathFiles()
        {
            try
            {
                if (Directory.GetFiles(PathEx.TmpPath).Length > 0)
                {
                    DirectoryEx.Delete(PathEx.TmpPath, true);
                    Directory.CreateDirectory(PathEx.TmpPath);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}
