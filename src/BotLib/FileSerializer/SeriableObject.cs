using BotLib.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.FileSerializer
{
    public class SeriableObject
    {
        public static ConcurrentDictionary<ISeriableObject, bool> ObjectSet = new ConcurrentDictionary<ISeriableObject, bool>();
  
        public static void DealAppClose()
        {
            new Thread(new ThreadStart(DealAppCloseInner))
            {
                IsBackground = false
            }.Start();
        }

        private static void DealAppCloseInner()
        {
            foreach (var kv in ObjectSet.xSafeForEach())
            {
                kv.Key.OnAppClose();
            }
        }

    }

    public class SeriableObject<T> : ISeriableObject where T : new()
    {
        public bool IsDirty = false;
        private bool _encrypt;
        private string _filename;
        private Func<T> _defaultIniter;
        private const int TimerIntervalBase = 60000;
        private Timer _timer;

        private bool _isTimerStoped = false;

        public T Data { get; set; }

        public SeriableObject(string name, Func<T> defaultIniter = null, bool autoSave = true, bool encrypt = true, bool saveOnlyDirty = true)
        {
            _filename = name;
            _encrypt = encrypt;
            _defaultIniter = defaultIniter;
            try
            {
                Data = Serializer.DeserializeFromFile<T>(name, encrypt);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            try
            {
                if (Data == null && defaultIniter != null)
                {
                    Data = defaultIniter();
                }
                Util.Assert(Data != null);
                if (autoSave)
                {
                    int randTime = 60000 + RandomEx.Rand.Next(30000);
                    _timer = new Timer(new TimerCallback(TimerTick), null, randTime, randTime);
                }
                SeriableObject.ObjectSet[this] = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void SaveToFile(bool always = false)
        {
            if (!IsDirty && !always) return;

            IsDirty = false;
            try
            {
                Serializer.SerializeToFile(Data, _filename, _encrypt);
            }
            catch (Exception e)
            {
                IsDirty = true;
                Log.Exception(e);
            }

        }

        private void TimerTick(object state)
        {
            if (!_isTimerStoped)
            {
                SaveToFile(false);
            }
        }

        ~SeriableObject()
        {
            OnAppClose();
        }

        public void ClearAll()
        {
            Data = _defaultIniter();
            SaveToFile(true);
        }

        public void OnAppClose()
        {
            try
            {
                SaveToFile(false);
                _isTimerStoped = true;
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}
