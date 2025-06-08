using System;
using System.Collections.Generic;
using System.Linq;
using BotLib;
using BotLib.Collection;
using BotLib.Extensions;
using Newtonsoft.Json;

namespace DbEntity
{
	public class UniformedString : IEquatable<UniformedString>
	{
		public readonly string Text;

		[JsonConstructor]
		private UniformedString(string text)
		{
			this.Text = text.xToBanJiaoAndToLowerAndSymplifiedAndTrim();
		}

		public static UniformedString Convert(string s)
		{
			return Cache.Get(s);
		}

		public static List<UniformedString> Convert(IEnumerable<string> strs)
		{
			return  strs.Select(k=>Convert(k)).ToList();
		}

		public override string ToString()
		{
			return this.Text;
		}

		public override int GetHashCode()
		{
			return this.Text.GetHashCode();
		}

		public bool Equals(UniformedString other)
		{
			bool rt = false;
			if (other != null)
			{
				if (Text == null)
				{
					rt = (other.Text == null);
				}
				else
				{
					rt = this.Text.Equals(other.Text);
				}
			}
			return rt;
		}

		private static class Cache
		{
			private static Cache<string, UniformedString> _cache = new Cache<string, UniformedString>(100000, 0, null);
			public static UniformedString Get(string txt)
			{
				UniformedString uniformedString = null;
				if (!_cache.TryGetValue(txt, out uniformedString, null))
				{
					uniformedString = new UniformedString(txt);
					_cache[txt] = uniformedString;
				}
				return uniformedString;
			}

		}
	}
}
