using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotLib.Extensions;

namespace BotLib.Db.Sqlite
{
    public class SQLiteHelper
    {
        private string _dbPath;
        private SQLiteConnection _conn;
        private object _synObj = new object();

        public SQLiteHelper(string dbpath, List<Type> initTables = null)
        {
            _dbPath = dbpath;
            if (initTables != null)
            {
                using (var conn = GetConnection())
                {
                    foreach (Type ty in initTables)
                    {
                        conn.CreateTable(ty, CreateFlags.None);
                    }
                }
            }
        }

        private SQLiteConnection GetConnection()
        {
            Monitor.Enter(_synObj);
            try
            {
                if (_conn == null)
                {
                    _conn = new SQLiteConnection(_dbPath, _synObj, false);
                    try
                    {
                        _conn.SetPragma();
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Exit(_synObj);
                throw ex;
            }
            return _conn;
        }

        public void SaveOneRecord(object obj)
        {
            using (var conn = GetConnection())
            {
                conn.InsertOrReplace(obj, obj.GetType());
            }
        }

        public void SaveRecordsInTransaction(List<object> olist)
        {
            using (var conn = GetConnection())
            {
                conn.RunInTransaction(() =>
                {
                    foreach (var obj in olist)
                    {
                        conn.InsertOrReplace(obj);
                    }
                });
            }
        }

        public T ReadOneRecord<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            T t;
            using (var conn = GetConnection())
            {
                try
                {
                    t = conn.Table<T>().Where(predicate).FirstOrDefault();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    t = default(T);
                }
            }
            return t;
        }

        public object ReadOneRecordByPropertyValue(Type type, string propName, object value)
        {
            object et = null;
            using (var conn = GetConnection())
            {
                var map = new TableMapping(type, CreateFlags.None);
                var sql = string.Format("select * from {0} where {1}={2}", type.Name, propName, SQLiteHelper.GetSqlStr(value));
                var ets = conn.Query(map, sql, new object[0]);
                if (ets != null && ets.Count > 0)
                {
                    et = ets[0];
                }
            }
            return et;
        }

        public object ReadOneRecordByPropertyValue(Type type, Dictionary<string, object> dict)
        {
            var ets = ReadMultiRecordByPropertyValue(type, dict, 1);
            return (ets != null && ets.Count > 0) ? ets[0] : null;
        }

        public List<T> ReadMultiRecordByPropertyValue<T>(string propName, string value)
        {
            var dict = new Dictionary<string, object>();
            dict[propName] = value;
            return ReadMultiRecordByPropertyValue(typeof(T), dict, 0).ConvertAll<T>((object x) => (T)((object)x));
        }

        public List<object> ReadMultiRecordByPropertyValue(Type type, Dictionary<string, object> dict, int limits = 0)
        {
            using (var conn = GetConnection())
            {
                var map = new TableMapping(type, CreateFlags.None);
                var sql = string.Format("select * from {0}", type.Name);
                if (dict != null && dict.Count > 0)
                {
                    sql += " where ";
                    foreach (var kv in dict)
                    {
                        sql = string.Concat(sql,kv.Key,"=",SQLiteHelper.GetSqlStr(kv.Value)," and ");
                    }
                    sql = sql.xSlice(0, 5);
                }
                if (limits > 0)
                {
                    sql = sql + " limit 0," + limits;
                }
                return conn.Query(map, sql, new object[0]);
            }
        }

        public int Delete(object obj)
        {
            int rt = 0;
            if (obj != null)
            {
                using (var conn = GetConnection())
                {
                    rt = conn.Delete(obj);
                }
            }
            return rt;
        }

        public int Execute(string sql, params object[] args)
        {
            int rt = 0;
            if (!string.IsNullOrEmpty(sql))
            {
                using (var conn = GetConnection())
                {
                    rt = conn.Execute(sql,args);
                }
            }
            return rt;
        }

        public void ClearTable(List<Type> tables)
        {
            using (var conn = GetConnection())
            {
                foreach (Type t in tables)
                {
                    conn.DeleteAll(t);
                }
            }
        }

        public int DeleteInTransaction(List<object> olist)
        {
            int n = 0;
            if (olist != null && olist.Count > 0)
            {
                using (var conn = GetConnection())
                {
                    conn.RunInTransaction(() =>
                    {
                        foreach (var et in olist)
                        {
                            n += conn.Delete(et);
                        }
                    });
                }
            }
            return n;
        }

        public List<T> ReadRecords<T>(Expression<Func<T, bool>> predicate = null) where T : new()
        {
            List<T> ets = new List<T>();
            using (var conn = GetConnection())
            {
                try
                {
                    var tableQuery = conn.Table<T>();
                    if (predicate != null)
                    {
                        tableQuery = tableQuery.Where(predicate);
                    }
                    ets.AddRange(tableQuery);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
            return ets;
        }

        public List<object> Select(Type type, string query, params object[] args)
        {
            using (var conn = GetConnection())
            {
                var map = new TableMapping(type, CreateFlags.None);
                var sql = string.Format("select * from {0} ", type.Name);
                if (!string.IsNullOrEmpty(query))
                {
                    sql += query;
                }
                return conn.Query(map, sql, args);
            }
        }


        public List<T> Select<T>(string select) where T : new()
        {
            var ets = new List<T>();
            using (var conn = GetConnection())
            {
                try
                {
                    ets = conn.Query<T>(select, new object[0]);
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
            return ets;
        }

        public List<T> SelectLimitCount<T>(int count, string orderByField, bool descending) where T : new()
        {
            var sql = string.Format("select * from {0} order by {1} {2} limit 0,{3}", typeof(T).Name, orderByField, descending ? "desc" : "asc", count);
            return Select<T>(sql);
        }

        public List<object> ReadTable(Type type)
        {
            return ReadMultiRecordByPropertyValue(type, null, 0);
        }

        public List<T> ReadTable<T>()
        {
            var ets = ReadTable(typeof(T));
            return ets == null ? null : ets.ConvertAll<T>(k => (T)k);
        }

        public static string GetSqlStr(object v)
        {
            if (v is string) return "'" + v.ToString().Replace("'", "''") + "'";
            if (v is DateTime)
            {
                throw new Exception("用Long代替DateTime字段");
            }
            if (v is bool)
            {
                return ((bool)v) ? "1" : "0";
            }
            if (v.GetType().IsEnum)
            {
                return ((int)v).ToString();
            }
            if (v == null) 
                return null;
            else return v.ToString();
        }

        public bool ContainsRecord<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            return ReadOneRecord<T>(predicate) != null;
        }

    }
}
