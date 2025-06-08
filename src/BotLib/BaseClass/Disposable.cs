using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLib.BaseClass
{
    public abstract class Disposable : IDisposable
    {
        private object _disposeSynObj = new object();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void CleanUp_UnManaged_Resources()
        {
        }

        protected abstract void CleanUp_Managed_Resources();

        public bool IsDisposed { get; private set; }

        protected void Dispose(bool disposeManagedRes)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                try
                {
                    CleanUp_UnManaged_Resources();
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
                if (disposeManagedRes)
                {
                    try
                    {
                        CleanUp_Managed_Resources();
                    }
                    catch (Exception e2)
                    {
                        Log.Exception(e2);
                    }
                }
            }
        }

        ~Disposable()
        {
            Dispose(false);
        }

    }
}
