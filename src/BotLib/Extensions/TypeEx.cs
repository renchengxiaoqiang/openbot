using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public static class TypeEx
    {
        public static object xGetDefaultValue(this Type t)
        {
            object obj = null;
            if (t.IsValueType)
            {
                obj = Activator.CreateInstance(t);
            }
            return obj;
        }
    }
}
