using BotLib.Collection;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotLib
{
    public class Util
    {
        private static Encoding _encodingGb2312 = Encoding.GetEncoding("gb2312");
        private static JsonSerializerSettings _serializeTypeNameSetting = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private static JsonSerializerSettings _serializeNoTypeSetting = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static Encoding EncodingGb2312
        {
            get
            {
                return _encodingGb2312;
            }
        }
        private static object _writeTraceSynObj = new object();
        private static int _lineCount = 1;
        private const bool IsWriteTraceToLog = false;
        private static DateTime _preWritaTraceTime = DateTime.MinValue;
        private static Cache<string, string> _normalizeStringCache = new Cache<string, string>(100, 0, null);

        public static bool Assert(bool condition, string msg = "", [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            if (!condition)
            {
                msg = string.Format("Assert false, msg={0},caller={1},path={2},line={3}", new object[]
				{
					msg,
					caller,
					PathEx.ConvertToRelativePath(path),
					line
				});
                Log.Assert(msg);
                throw new Exception(msg);
            }
            return condition;
        }

        public static void ThrowException(string msg = "", [System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerFilePath] string path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            msg = string.Format("ThrowException, msg={0},caller={1},path={2},line={3}",msg,caller,PathEx.ConvertToRelativePath(path),line);
            Log.Error(msg);
            throw new Exception(msg);
        }

        public static void Nav(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                url = "about:blank";
            }
            try
            {
                Process process = Process.Start(url);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                try
                {
                    var msg = "找不到【默认浏览器】。\r\n\r\n若要将网址复制到系统【剪贴板】，请选按钮“是”。然后手动打开浏览器，粘贴网址，打开网页。";
                    if (System.Windows.Forms.MessageBox.Show(msg, "无法打开网页", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        System.Windows.Clipboard.SetDataObject(url);
                    }
                }
                catch (Exception e2)
                {
                    Log.Exception(e2);
                }
            }
        }

        public static ReturnType Clone<ReturnType>(object ent)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new MyContractResolver(),
                TypeNameHandling = TypeNameHandling.All
            };
            string value = JsonConvert.SerializeObject(ent, settings);
            return (ReturnType)((object)JsonConvert.DeserializeObject(value, ent.GetType(), settings));
        }

        public static void Beep()
        {
            SystemSounds.Beep.Play();
        }

        public static string SerializeWithTypeName(object obj)
        {
            return Serialize(obj, _serializeTypeNameSetting);
        }

        public static string SerializeNoTypeName(object obj)
        {
            return Serialize(obj, _serializeNoTypeSetting);
        }

        private static string Serialize(object obj, JsonSerializerSettings setting)
        {
            Type ty = obj.GetType();
            return (ty.IsPrimitive || ty.IsEnum || ty == typeof(string) || ty == typeof(DateTime)) ? obj.ToString() : JsonConvert.SerializeObject(obj, setting);
        }

        public static T DeserializeWithTypeName<T>(string s)
        {
            return Deserialize<T>(s, _serializeTypeNameSetting);
        }

        public static T DeserializeNoTypeName<T>(string s)
        {
            return Deserialize<T>(s, _serializeNoTypeSetting);
        }

        private static T Deserialize<T>(string s, JsonSerializerSettings settings)
        {
            Type ty = typeof(T);
            try
            {
                if (ty == typeof(string)) return (T)((object)s);
                if (ty.IsEnum)
                {
                    return (T)((object)Enum.Parse(typeof(T), s));
                }
                return (ty.IsPrimitive || ty == typeof(DateTime)) ? (T)((object)Convert.ChangeType(s, ty)) : JsonConvert.DeserializeObject<T>(s, settings);
            }
            catch (Exception ex)
            {
                try
                {
                    return (T)((object)Convert.ChangeType(s, ty));
                }
                catch (Exception ex2)
                {
                    Log.Exception(ex2);
                }
                Log.Exception(ex);
            }
            return default(T);
        }

        public static void SleepWithDoEvent(int ms)
        {
            Assert(ms > 0);
            int millisecondsTimeout = Math.Min(30, ms);
            DateTime now = DateTime.Now;
            do
            {
                Thread.Sleep(millisecondsTimeout);
                DispatcherEx.DoEvents();
            }
            while ((DateTime.Now - now).TotalMilliseconds < (double)ms);
        }

        public static void WriteTrace(object v)
        {
            WritaTrace(JsonConvert.SerializeObject(v));
        }

        public static void WritaTrace(string v)
        {
            if (Params.IsDevoloperClient)
            {
                lock (_writeTraceSynObj)
                {
                    var ms = (_preWritaTraceTime == DateTime.MinValue) ? 0.0 : (DateTime.Now - _preWritaTraceTime).TotalMilliseconds;
                    _preWritaTraceTime = DateTime.Now;
                    var message = string.Format("{0}({1},{2}):{3}", _lineCount,DateTime.Now.ToString("MM:ss"),ms.ToString("0.000"),v);
                    Trace.WriteLine(message);
                    _lineCount++;
                }
            }
        }

        public static void WriteTrace(string format, params object[] args)
        {
            WritaTrace(string.Format(format, args));
        }

        public static bool WaitFor(Func<bool> pred, int timeoutMs = 0, int testIntervalMs = 10, bool withDoEvent = false)
        {
            if (testIntervalMs < 1)
            {
                testIntervalMs = 10;
            }
            DateTime now = DateTime.Now;
            var isTimeout = false;
            while (true)
            {
                if (pred())
                {
                    break;
                }
                if (timeoutMs > 0 && now.xIsTimeElapseMoreThanMs(timeoutMs))
                {
                    isTimeout = true;
                    break;
                }
                if (withDoEvent)
                {
                    DispatcherEx.DoEvents();
                }
                Thread.Sleep(testIntervalMs);
            }
            return isTimeout;
        }

        public static T CallWithoutException<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return default(T);
        }

        public static void CallWithoutException(Action act)
        {
            try
            {
                act();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void CallOnceAfterDelayInUiThread(Action act, int delayMs)
        {
            DelayCaller.CallAfterDelayInUIThread(act, delayMs);
        }

        public static bool IsProcessRuning(string name)
        {
            Process[] processesByName = Process.GetProcessesByName(name);
            return !processesByName.xIsNullOrEmpty();
        }

        public static void WriteTimeElapsed(DateTime t0, string msg = "")
        {
            WriteTrace("time elapsed={1} ms,{0}",msg,(DateTime.Now - t0).TotalMilliseconds);
        }

        public class MyContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
			{
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Select(p => base.CreateProperty(p, memberSerialization)).Union(type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(f => base.CreateProperty(f, memberSerialization))).ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
			}
        }
    }
}
