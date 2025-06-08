using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public class StreamReaderEx
    {
        public static void OpenSteamForRead(string filePath, Encoding enc, Action<StreamReader> act)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs, enc))
                {
                    act(sr);
                }
            }
        }
    }
}
