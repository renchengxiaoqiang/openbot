using BotLib;
using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Common.Db
{
    public class HybridHelper
    {
        private const string _keyLinkerStr = "#-#";

        public static void Save<T>(string dbAccount, string key, T value)
		{
			var et = DbHelper.FirstOrDefault<HybridEntity>(dbAccount, k=>k.Key == key);
			var text = Util.SerializeWithTypeName(value);
			if (et == null)
			{
				et = EntityHelper.Create<HybridEntity>(dbAccount);
				et.Key = key;
				et.Value = text;
                DbHelper.SaveToDb(et);
			}
			else if (et.Value != text)
			{
				et.Value = text;
                DbHelper.SaveToDb(et);
			}
		}

        public static void Save<T>(string dbAccount, string masterkey, string subkey, T value)
        {
            Save(dbAccount, GetKey(masterkey, subkey), value);
        }

        public static T GetValue<T>(string dbAccount, string key, T defVal = default(T))
		{
			T t = defVal;
			var et = DbHelper.FirstOrDefault<HybridEntity>(dbAccount,k=>k.Key == key);
			if (et != null)
			{
				t = Util.DeserializeWithTypeName<T>(et.Value);
			}
			return t;
		}

        public static T GetValue<T>(string dbAccount, string masterkey, string subkey, T value = default(T))
        {
            return GetValue(dbAccount, GetKey(masterkey, subkey), value);
        }

        private static string GetKey(string masterkey, string subkey)
        {
            return masterkey + _keyLinkerStr + subkey;
        }
    }
}
