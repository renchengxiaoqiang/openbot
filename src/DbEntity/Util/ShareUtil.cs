   using System;
namespace DbEntity
{
	public class ShareUtil
	{
		public static string ConvertVersionToString(int v)
		{
			int v1 = v / 10000;
			int v2 = v % 10000 / 100;
			int v3 = v % 100;
			return string.Format("v{0}.{1}.{2}", v1, v2, v3);
		}

		public static int ConvertStringToVersion(string vstr)
		{
			vstr = vstr.Trim().ToLower();
			var vs = vstr.Split('.');
			int v1 = Convert.ToInt32(vs[0].Substring(1));
			int v2 = Convert.ToInt32(vs[1]);
			int v3 = Convert.ToInt32(vs[2]);
			return v1 * 10000 + v2 * 100 + v3;
		}
	}
}
