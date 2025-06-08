using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class DateTimeEx
    {
        public static bool xIsHourMiniteWithin(this DateTime t, int hourBegin, int minuteBegin, int hourEnd, int MiniteEnd)
        {
            return t.xIsHourMiniteLargeThan(hourBegin, minuteBegin) && t.xIsHourMiniteLessThan(hourEnd, MiniteEnd);
        }

        public static bool xIsHourMiniteLargeThan(this DateTime t, int hourBegin, int minuteBegin)
        {
            return t.Hour > hourBegin || (t.Hour == hourBegin && t.Minute > minuteBegin);
        }

        public static bool xIsHourMiniteLessThan(this DateTime t, int hourEnd, int minuteEnd)
        {
            return t.Hour < hourEnd || (t.Hour == hourEnd && t.Minute < minuteEnd);
        }

        public static bool xIsTimeElapseMoreThanMs(this DateTime start, int ms)
        {
            return (DateTime.Now - start).TotalMilliseconds > (double)ms;
        }

        public static bool xIsTimeElapseMoreThanSecond(this DateTime start, int sec)
        {
            return (DateTime.Now - start).TotalSeconds > (double)sec;
        }

        public static bool xIsTimeElapseMoreThanMinute(this DateTime start, double minute)
        {
            return (DateTime.Now - start).TotalMinutes > minute;
        }

        public static bool xIsTimeElapseMoreThanDays(this DateTime start, double days)
        {
            return (DateTime.Now - start).TotalDays > days;
        }

        public static bool xIsTimeElapseMoreThanHours(this DateTime start, double hours)
        {
            return (DateTime.Now - start).TotalHours > hours;
        }

        public static string xToString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string xToString2(this DateTime dateTime)
        {
            return dateTime.ToString("yy年M月d日H时m分s秒");
        }

        public static DateTime? ParseDateTimeForToString2(string txt)
        {
            DateTime? date = null;
            try
            {
                var pattern = "\\d+年\\d+月\\d+日\\d+时\\d+分\\d+秒";
                Util.Assert(Regex.IsMatch(txt, pattern), "日期格式出错（yy年M月d日H时m分s秒）:" + txt);
                var patternDateNumber = "\\d+";
                var numbers = Regex.Matches(txt, patternDateNumber);
                Util.Assert(numbers.Count == 6, "日期格式出错（yy年M月d日H时m分s秒）:" + txt);
                int year = Convert.ToInt32(numbers[0].ToString());
                if (year < 100)
                {
                    year += 2000;
                }
                int month = Convert.ToInt32(numbers[1].ToString());
                int day = Convert.ToInt32(numbers[2].ToString());
                int hour = Convert.ToInt32(numbers[3].ToString());
                int minute = Convert.ToInt32(numbers[4].ToString());
                int second = Convert.ToInt32(numbers[5].ToString());
                date = new DateTime?(new DateTime(year, month, day, hour, minute, second));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return date;
        }

        public static long xToTimeStamp(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        public static DateTime xTimeStampToDate(this long timeStamp)
        {
            var dt = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
            return dt.AddTicks(timeStamp * 10000);
        }

        public static string xToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yy年M月d日");
        }

        public static DateTime ParseSafety(string timeStr)
        {
            var dt = DateTime.MinValue;
            try
            {
                dt = Convert.ToDateTime(timeStr.Trim());
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return dt;
        }

        public static string xTodayDayString
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        public static TimeSpan xElapse(this DateTime t0)
        {
            return DateTime.Now - t0;
        }
    }
}
