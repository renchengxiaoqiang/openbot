using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Extensions
{
    public class RandomEx
    {
        public static Random Rand = new Random();

        public static T NextEnum<T>() where T : struct, IConvertible
		{
			Type typeFromHandle = typeof(T);
			if (!typeFromHandle.IsEnum)
			{
				throw new InvalidOperationException();
			}
			Array values = Enum.GetValues(typeFromHandle);
			int index = RandomEx.Rand.Next(values.GetLowerBound(0), values.GetUpperBound(0) + 1);
			return (T)((object)values.GetValue(index));
		}

        public static bool NextBool()
        {
            return RandomEx.Rand.NextDouble() > 0.5;
        }

        public static byte[] NextBytes(int length)
        {
            byte[] array = new byte[length];
            RandomEx.Rand.NextBytes(array);
            return array;
        }

        public static double Next
        {
            get
            {
                return RandomEx.Rand.NextDouble();
            }
        }

        public static int NextInt(int max)
        {
            return RandomEx.Rand.Next(max);
        }

        public static DateTime NextDateTime(DateTime minValue, DateTime maxValue)
        {
            long ticks = minValue.Ticks + (long)((double)(maxValue.Ticks - minValue.Ticks) * RandomEx.Rand.NextDouble());
            return new DateTime(ticks);
        }

        public static DateTime NextDateTime()
        {
            return RandomEx.NextDateTime(DateTime.MinValue, DateTime.MaxValue);
        }
    }
}
