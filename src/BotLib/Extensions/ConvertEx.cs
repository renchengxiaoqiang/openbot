using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public class ConvertEx
    {
        public static double ToDoubleSafe(object input, double def = 0.0)
        {
            try
            {
                return Convert.ToDouble(input);
            }
            catch (Exception ex)
            {
                Log.Error(string.Concat("Convert.ToDouble,input=",input,",exp=",ex.Message));
                Log.StackTrace();
            }
            return def;
        }

        public static int ToInt32Safe(string text, int defv)
        {
            try
            {
                defv = Convert.ToInt32(text);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return defv;
        }

        public static long ToInt64Safe(string text, long defv = 0L)
        {
            try
            {
                defv = Convert.ToInt64(text);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return defv;
        }

        public static bool ToInt64(string text, out long val)
        {
            bool rt = false;
            val = 0L;
            try
            {
                val = Convert.ToInt64(text);
                rt = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }
    }
}
