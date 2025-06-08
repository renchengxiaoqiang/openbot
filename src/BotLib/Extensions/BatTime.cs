using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BotLib.Extensions
{
    public static class BatTime
    {
        public static bool IsOk = false;
        private static NoReEnterTimer _timer;
        private static long _startTick = 0L;
        private static ManualResetEventSlim _reslim = new ManualResetEventSlim(false);

        private static DateTime _minTime = new DateTime(2017, 10, 26);
        public static bool IsTimeLargerThanBatOver5Second(DateTime t)
        {
            return IsOk && (t - Now).TotalSeconds > 5.0;
        }

        public static bool IsTimeLargerThanBatOver20Second(DateTime t)
        {
            return IsOk && (t - Now).TotalSeconds > 20.0;
        }

        public static bool IsTimeLargerThanBatOver10Minute(DateTime t)
        {
            return IsOk && (t - Now).TotalMinutes > 10.0;
        }

        public static string xGetBatTimeElapseDesc(this DateTime start)
        {
            var timeSpan = Now - start;
            if (timeSpan.TotalDays > 365.0) return (int)(timeSpan.TotalDays / 365.0) + "年";
            if (timeSpan.TotalDays >= 30.0) return (int)(timeSpan.TotalDays / 30.0) + "个月";
            if (timeSpan.TotalDays >= 1.0) return (int)timeSpan.TotalDays + "天";
            if (timeSpan.TotalHours >= 1.0) return (int)timeSpan.TotalHours + "小时";
            if (timeSpan.TotalMinutes >= 1.0) return (int)timeSpan.TotalMinutes + "分钟";
            int second = (int)timeSpan.TotalSeconds;
            if (second <= 0)
            {
                second = 1;
            }
            return second + "秒";
        }

        public static long NowUtcTicks
        {
            get
            {
                return Now.ToUniversalTime().Ticks;
            }
        }

        public static TimeSpan Diff(DateTime t0)
        {
            return Now - t0;
        }

        public static DateTime Now
        {
            get
            {
                return IsOk ? new DateTime(_startTick + StopwatchEx.ElapsedTicks) : DateTime.Now;
            }
        }

        public static string NowString
        {
            get
            {
                return Now.xToString();
            }
        }

        public static bool Now2(out DateTime n)
        {
            n = Now;
            return IsOk;
        }

        static BatTime()
        {
            _timer = new NoReEnterTimer(new Action(Loop), 3000, 0);
        }

        public static void WaitForInit(int timeoutMs)
        {
            if (timeoutMs < 0)
            {
                timeoutMs = 0;
            }
            _reslim.Wait(timeoutMs);
        }

        private static bool InitTimeFromServer()
        {
            bool rt = false;
            _reslim.Reset();
            try
            {
                DateTime dateTime;
                bool timeFromBatServer = GetTimeFromBatServer(out dateTime);
                if (timeFromBatServer)
                {
                    _startTick = dateTime.ToUniversalTime().AddHours(8.0).Ticks - StopwatchEx.ElapsedTicks;
                    IsOk = true;
                    rt = true;
                    Log.Info("BatTime Init Ok=" + dateTime.xToString());
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            _reslim.Set();
            return rt;
        }

        private static void Loop()
        {
            try
            {
                if (InitTimeFromServer())
                {
                    _timer.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static bool CanAccessBat
        {
            get
            {
                DateTime dateTime;
                return  GetTimeFromBatServer(out dateTime);
            }
        }

        private static bool GetTimeFromBatServer(out DateTime dt)
        {
            var reqUrls = new List<string>
			{
				"http://www.taobao.com",
			};
            dt = DateTime.Now;
            bool rt = false;
            //var getDtTasks = new List<Task<DateTime?>>();
            //reqUrls.ForEach(url =>
            //{
            //    var dtTask = Task.Factory.StartNew(() => GetTimeFromWebServer(url), TaskCreationOptions.LongRunning);
            //    getDtTasks.Add(dtTask);
            //});
            //while (getDtTasks.Count > 0)
            //{
            //    var tasks = getDtTasks.ToArray();
            //    int idx = Task.WaitAny(tasks);
            //    if (idx >= 0)
            //    {
            //        var svrDt = getDtTasks[idx].Result;
            //        if (svrDt.HasValue)
            //        {
            //            dt = svrDt.Value;
            //            if (dt != default(DateTime))
            //            {
            //                rt = true;
            //                break;
            //            }
            //        }
            //    }
            //    getDtTasks.RemoveAt(idx);
            //}
            return rt;
        }

        private static DateTime? GetTimeFromWebServer(string url)
        {
            DateTime? dateTime = null;
            try
            {
                var req = WebRequest.Create(url);
                req.Method = "get";
                var res = (HttpWebResponse)req.GetResponse();
                dateTime = Convert.ToDateTime(res.Headers.Get("Date"));
                if (dateTime < _minTime)
                {
                    throw new Exception("下载的时间太小,time=" + dateTime.Value.xToString());
                }
                if ((dateTime.Value - _minTime).Days > 1800)
                {
                    Log.Info(string.Format("下载到的时间似乎太大，time={0}", dateTime.Value.xToString()));
                }
                res.Close();
            }
            catch (Exception ex)
            {
                dateTime = null;
                Log.Error("WebServer出错:" + url + ",exp=" + ex.Message);
            }
            return dateTime;
        }
    }

}
