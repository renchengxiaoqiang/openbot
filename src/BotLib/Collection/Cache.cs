using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.Collection
{
    public class Cache<KT, VT>
    {
        private ConcurrentDictionary<KT, Data> _dict = new ConcurrentDictionary<KT, Data>();
        private Action<VT> _disposeDealer;
        private int _minCacheSize;
        private int _maxCacheSize;
        private int _timeoutMs;
        private long accessCount = 0L;
        private long hitCount = 0L;
        private object _removingLazyKeySynObj = new object();
        private bool _isRemovingLazykey = false;

        public Cache(int size = 0, int timeoutMs = 0, Action<VT> disposeDealer = null)
        {
            if (size <= 0)
            {
                size = 1073741823;
            }
            _disposeDealer = disposeDealer;
            _minCacheSize = size;
            _maxCacheSize = size * 2;
            _timeoutMs = timeoutMs;
        }

        public VT this[KT key]
        {
            get
            {
                VT result;
                TryGetValue(key, out result, default(VT));
                return result;
            }
            set
            {
                AddOrUpdate(key, value);
            }
        }

        public bool ContainsKey(KT key)
        {
            return _dict.ContainsKey(key);
        }

        public bool TryGetValue(KT key, out VT value, VT defv = default(VT))
        {
            accessCount += 1L;
            value = defv;
            bool rt = false;
            if (key != null)
            {
                Data data;
                if (_dict.TryGetValue(key, out data))
                {
                    if (data.IsTimeout(_timeoutMs))
                    {
                        rt = false;
                        Remove(key);
                    }
                    else
                    {
                        value = data.TheData;
                        data.LastAccessTime = DateTime.Now;
                        hitCount += 1L;
                    }
                }
            }
            return rt;
        }

        public VT GetValue(KT key, Func<VT> producer, bool useCache = true, Predicate<VT> needCache = null)
        {
            VT vt = default(VT);
            accessCount += 1L;
            if (useCache && TryGetValue(key, out vt, default(VT)))
            {
                hitCount += 1L;
            }
            else
            {
                vt = producer();
                if (needCache == null || needCache(vt))
                {
                    AddOrUpdate(key, vt);
                }
            }
            return vt;
        }

        public bool IsCacheTimeElapseMoreThanMs(KT key, int timeoutMs)
        {
            bool rt = true;
            Data data;
            if (_dict.TryGetValue(key, out data))
            {
                rt = data.IsTimeout(timeoutMs);
            }
            return rt;
        }

        public void RemoveItems(Predicate<VT> predict)
        {
            _dict.Where(k => predict(k.Value.TheData)).ToList().ForEach(k => { Remove(k.Key); });
        }

        public double HitRate()
        {
            double rt;
            if (accessCount == 0L)
            {
                rt = 0.0;
            }
            else
            {
                rt = (double)hitCount / (double)accessCount;
            }
            return rt;
        }

        public void Clear()
        {
            var dict = _dict;
            _dict = new ConcurrentDictionary<KT, Data>();
            if (_disposeDealer != null)
            {
                foreach (KT key in dict.Keys)
                {
                    Data data;
                    if (dict.TryRemove(key, out data))
                    {
                        try
                        {
                            _disposeDealer(data.TheData);
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                }
            }
        }

        public void AddOrUpdate(KT key, VT value)
        {
            if (key != null)
            {
                if (_dict.ContainsKey(key))
                {
                    DisposeData(_dict[key].TheData);
                }
                else
                {
                    RemoveLazyKeyIfNeed();
                }
                _dict[key] = new Data(value);
            }
        }

        private void DisposeData(VT v)
        {
            try
            {
                if (_disposeDealer != null)
                {
                    _disposeDealer(v);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void Remove(KT key)
        {
            Data data;
            if (_dict.TryRemove(key, out data))
            {
                DisposeData(data.TheData);
            }
        }

        private void RemoveLazyKeyIfNeed()
        {
            bool isRemovingLazykey = _isRemovingLazykey;
            if (!isRemovingLazykey)
            {
                lock (_removingLazyKeySynObj)
                {
                    _isRemovingLazykey = true;
                    if (_dict.Count > _maxCacheSize)
                    {
                        try
                        {
                            var dict = _dict;
                            _dict = new ConcurrentDictionary<KT, Data>();
                            dict.OrderByDescending(k => k.Value.LastAccessTime)
                                .Skip(_minCacheSize).Select(k => k.Key).ToList<KT>()
                                .ForEach(k =>
                                {
                                    Data data;
                                    if (dict.TryRemove(k, out data) && _disposeDealer != null)
                                    {
                                        try
                                        {
                                            _disposeDealer(data.TheData);
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Exception(e);
                                        }
                                    }
                                });
                            _dict = dict;
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                    _isRemovingLazykey = false;
                }
            }
        }

        private class Data
        {
            public VT TheData;

            public DateTime LastAccessTime;

            public DateTime CacheTime;

            public Data(VT d)
            {
                TheData = d;
                LastAccessTime = DateTime.Now;
                CacheTime = LastAccessTime;
            }

            public void UpdateAccessTime()
            {
                LastAccessTime = DateTime.Now;
            }

            internal bool IsTimeout(int timeoutMs)
            {
                return timeoutMs > 0 && (DateTime.Now - CacheTime).TotalMilliseconds > (double)timeoutMs;
            }

        }
    }
}
