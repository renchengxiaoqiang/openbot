using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib
{
    public class Log
    {
        private static LogWriter _writer;
        private static ConcurrentDictionary<string, int> _errorWithMaxCountDict = new ConcurrentDictionary<string, int>();

        private static LogWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    Initiate("", false, 0, true);
                }
                return _writer;
            }
        }

        public static void WriteEnvironmentString(string tip)
        {
            _writer.WriteEnvironmentString(tip);
        }

        public static void Initiate(string FileName = "", bool saveLogByDay = false, int maxByte = 0, bool limitSameStringWriteCount = true)
        {
            if (_writer != null)
            {
                _writer.Close("启动程序");
            }
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = "txt";
            }
            if (!FileName.ToLower().EndsWith(".txt"))
            {
                FileName += ".txt";
            }
            if (!FileName.Contains("\\"))
            {
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,FileName);
            }
            _writer = new LogWriter(FileName, saveLogByDay, maxByte);
            _writer.LimitSameStringWriteCount = limitSameStringWriteCount;
        }

        public static void Assert(string msg)
        {
            Writer.Assert(msg);
        }

        public static void Clear()
        {
            Writer.Clear();
        }

        public static void Error(string msg, object o, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            msg = msg + Environment.NewLine + "data=" + JsonConvert.SerializeObject(o);
            Writer.Error(GetDesc(msg, caller, path, line));
        }

        public static void Error(string msg, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            Writer.Error(GetDesc(msg, caller, path, line));
        }

        public static void ErrorWithMaxCount(string msg, int maxCount = 5, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            string key = caller + line + path;
            if (IsLogCountLessThanMaxCount(key, maxCount + 1))
            {
                Writer.Error(GetDesc(msg, caller, path, line));
            }
        }

        private static bool IsLogCountLessThanMaxCount(string key, int maxCount)
        {
            int cnt = _errorWithMaxCountDict.GetOrAdd(key, 0);
            cnt++;
            if (cnt < maxCount)
            {
                _errorWithMaxCountDict[key] = cnt;
            }
            return cnt < maxCount;
        }

        private static string GetDesc(string msg, string caller, string path, int line)
        {
            var idx = path.LastIndexOf('\\');
            path = path.Substring(idx);
            if (!msg.Contains(caller) && msg.Contains(path))
            {
                msg = string.Format("{0}\r\ncaller={1}, file={2},line={3}", msg.Trim(), caller, path, line);
            }
            return msg;
        }

        private static string GetDesc(Exception e, string caller, string path, int line)
        {
            var idx = path.LastIndexOf('\\');
            path = path.Substring(idx);
            string text = e.ToString();
            if (!text.Contains(caller) && text.Contains(path))
            {
                text = string.Format("Message={0}\r\nBreakPoint={3}\r\ncaller={1}, file={2},line={3}\r\nStackTrace={4}",
					text.Trim(),
					caller,
					path,
					line,
					e.StackTrace
				);
            }
            return text;
        }

        public static void Exception(Exception e, [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            Writer.Exception(GetDesc(e, caller, path, line));
        }

        public static void Info(string text)
        {
            Writer.Info(text);
        }

        public static void Debug(string text)
        {
            Writer.Debug(text);
        }

        public static void Show()
        {
            Writer.Show();
        }

        public static void TimeElapse(string title, DateTime t0)
        {
            Writer.TimeElapse(title, t0);
        }

        public static void WriteLine(string format, params object[] args)
        {
            try
            {
                string msg = string.Format(format, args);
                Writer.WriteLine(msg);
            }
            catch (Exception e)
            {
                Exception(e);
            }
        }

        public static void StackTrace()
        {
            Writer.StackTrace();
        }

        public static void Close(string reason = "")
        {
            Writer.Close(reason);
        }

        public static string CopyTo(string fn)
        {
            string rt = null;
            try
            {
                Writer.CopyTo(fn);
                if (!File.Exists(fn))
                {
                    Error("无法复制文件到:" + fn);
                }
            }
            catch (Exception ex)
            {
                Exception(ex);
                Info("fn=" + fn);
                rt = ex.Message;
            }
            return rt;
        }

    }
}
