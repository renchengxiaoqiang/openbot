using BotLib.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Db.Sqlite
{
    public class PersistentParams
    {
        private static ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private static SQLiteHelper _db;
        private const string _keyLinkerStr = "#-#";
        private static void ClearCache()
        {
            _cache = new ConcurrentDictionary<string, string>();
        }

        static PersistentParams()
        {
            var dbpath = PathEx.DataDir + "params.db";
            _db = new SQLiteHelper(dbpath, new List<Type>
			{
				typeof(ParamItem)
			});
            InitParam();
        }

        private static void InitParam()
        {
            try
            {
                var pms = _db.ReadRecords<ParamItem>(null);
                foreach (var paramItem in pms)
                {
                    _cache[paramItem.Key] = paramItem.Value;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void TrySaveParam(string key, string value)
        {
            try
            {
                if (!_cache.ContainsKey(key) || _cache[key] != value)
                {
                    _cache[key] = value;
                    var item = new ParamItem
                    {
                        Key = key,
                        Value = value
                    };
                    _db.SaveOneRecord(item);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static void TrySaveParam<T>(string key, T value)
        {
            var val = Util.SerializeWithTypeName(value);
            TrySaveParam(key, val);
        }

        public static void TrySaveParam2Key<T>(string masterKey, string subKey, T value)
        {
            TrySaveParam<T>(GetKey(masterKey, subKey), value);
        }

        public static void TrySaveParam(string key, DateTime time)
        {
            TrySaveParam(key, time.Ticks.ToString());
        }

        public static void TrySaveParam(string key, double value)
        {
            TrySaveParam(key, value.ToString());
        }

        public static void TrySaveParam(string key, int value)
        {
            TrySaveParam(key, value.ToString());
        }

        public static void TrySaveParam(string key, long value)
        {
            TrySaveParam(key, value.ToString());
        }

        public static void TrySaveParam(string key, bool value)
        {
            TrySaveParam(key, value.ToString());
        }

        public static void Delete2KeyParamsByMasterKey(string mk)
        {
            DeleteParamsForKeyBeginWith(GetKey(mk, ""));
        }

        private static void DeleteParamsForKeyBeginWith(string keyInit)
        {
            ClearCache();
            try
            {
                var pms = _db.ReadRecords<ParamItem>(k => k.Key.StartsWith(keyInit)).ConvertAll<object>(k => k);
                if (pms.xCount() > 0)
                {
                    _db.DeleteInTransaction(pms);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static List<string> GetKeysOf2KeyParamsByMasterKey(string mk)
        {
            string init = GetKey(mk, "");
            return _db.ReadRecords<ParamItem>(k => k.Key.StartsWith(init)).ConvertAll<string>(k => k.Key);
        }

        public static List<string> GetValuesOf2KeyParamsByMasterKey(string mk)
        {
            string init = GetKey(mk, "");
            return _db.ReadRecords<ParamItem>(k => k.Key.StartsWith(init)).ConvertAll<string>(k => k.Value);
        }

        public static Dictionary<string, string> Get2KeyParamsByMasterKey(string mk)
        {
            var rtdict = new Dictionary<string, string>();
            string init = GetKey(mk, "");
            _db.ReadRecords<ParamItem>(k => k.Key.StartsWith(init)).ForEach( item =>
            {
                rtdict[item.Key] = item.Value;
            });
            return rtdict;
        }

        public static void TrySaveParam2Key(string masterKey, string subKey, bool value)
        {
            TrySaveParam(GetKey(masterKey, subKey), value);
        }

        public static void TrySaveParam2Key(string masterKey, string subKey, long value)
        {
            TrySaveParam(GetKey(masterKey, subKey), value);
        }

        public static void TrySaveParam2Key(string masterKey, string subKey, double value)
        {
            TrySaveParam(GetKey(masterKey, subKey), value);
        }

        public static void TrySaveParam2Key(string masterKey, string subKey, int value)
        {
            TrySaveParam(GetKey(masterKey, subKey), value);
        }

        public static void TrySaveParam2Key(string masterKey, string subKey, DateTime value)
        {
            TrySaveParam(GetKey(masterKey, subKey), value);
        }

        public static void TrySaveParam2Key(string masterKey, string subKey, string value)
        {
            TrySaveParam(GetKey(masterKey, subKey), value);
        }

        public static string GetParam(string key, string defv = "")
        {
            if (_cache.ContainsKey(key))
            {
                defv = _cache[key];
            }
            return defv;
        }

        public static T GetParam<T>(string key, T defv)
        {
            string param = GetParam(key, "");
            if (!string.IsNullOrEmpty(param))
            {
                defv = Util.DeserializeWithTypeName<T>(param);
            }
            return defv;
        }

        public static T GetParam2Key<T>(string masterKey, string subKey, T defv)
        {
            return GetParam<T>(GetKey(masterKey, subKey), defv);
        }

        public static int GetParam(string key, int defv = 0)
        {
            if (_cache.ContainsKey(key))
            {
                try
                {
                    defv = Convert.ToInt32(_cache[key]);
                }
                catch
                {
                }
            }
            return defv;
        }

        public static double GetParam(string key, double defv = 0.0)
        {
            if (_cache.ContainsKey(key))
            {
                try
                {
                    defv = Convert.ToDouble(_cache[key]);
                }
                catch
                {
                }
            }
            return defv;
        }

        public static DateTime GetParam(string key, DateTime defv)
        {
            if (_cache.ContainsKey(key))
            {
                try
                {
                    defv = new DateTime(Convert.ToInt64(_cache[key]));
                }
                catch
                {
                }
            }
            return defv;
        }

        public static long GetParam(string key, long defv)
        {
            if (_cache.ContainsKey(key))
            {
                try
                {
                    defv = Convert.ToInt64(_cache[key]);
                }
                catch
                {
                }
            }
            return defv;
        }

        public static bool GetParam(string key, bool defv)
        {
            if (_cache.ContainsKey(key))
            {
                try
                {
                    defv = Convert.ToBoolean(_cache[key]);
                }
                catch
                {
                }
            }
            return defv;
        }

        public static bool GetParam2Key(string masterKey, string subKey, bool defv)
        {
            return GetParam(GetKey(masterKey, subKey), defv);
        }

        public static long GetParam2Key(string masterKey, string subKey, long defv)
        {
            return GetParam(GetKey(masterKey, subKey), defv);
        }

        public static DateTime GetParam2Key(string masterKey, string subKey, DateTime defv)
        {
            return GetParam(GetKey(masterKey, subKey), defv);
        }

        public static double GetParam2Key(string masterKey, string subKey, double defv)
        {
            return GetParam(GetKey(masterKey, subKey), defv);
        }

        public static int GetParam2Key(string masterKey, string subKey, int defv)
        {
            return GetParam(GetKey(masterKey, subKey), defv);
        }

        public static string GetParam2Key(string masterKey, string subKey, string defv)
        {
            return GetParam(GetKey(masterKey, subKey), defv);
        }

        public static Dictionary<string, string> GetParamsByMasterKeyOf2Key(string masterkey)
        {
            var dict = new Dictionary<string, string>();
            masterkey += "#-#";
            int length = masterkey.Length;
            foreach (var kv in _cache)
            {
                if (kv.Key.StartsWith(masterkey))
                {
                    string key = kv.Key.Substring(length);
                    dict[key] = kv.Value;
                }
            }
            return dict;
        }

        private static string GetKey(string masterKey, string subKey)
        {
            return masterKey + "#-#" + subKey;
        }

        private class ParamItem
        {
            [PrimaryKey]
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}
