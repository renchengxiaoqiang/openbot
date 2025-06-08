using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotLib.Collection
{
    public class ConcurrentHashSet<T> : IDisposable
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		private readonly HashSet<T> _hashSet = new HashSet<T>();
		public bool Add(T item)
		{
			bool rt;
			try
			{
				_lock.EnterWriteLock();
				rt = _hashSet.Add(item);
			}
			finally
			{
				if (_lock.IsWriteLockHeld)
				{
					_lock.ExitWriteLock();
				}
			}
			return rt;
		}

		public void Clear()
		{
			try
			{
				_lock.EnterWriteLock();
				_hashSet.Clear();
			}
			finally
			{
				if (_lock.IsWriteLockHeld)
				{
					_lock.ExitWriteLock();
				}
			}
		}

		public bool Contains(T item)
		{
			bool rt;
			try
			{
				_lock.EnterReadLock();
				rt = _hashSet.Contains(item);
			}
			finally
			{
                if (_lock.IsReadLockHeld)
				{
					_lock.ExitReadLock();
				}
			}
			return rt;
		}

		public bool Remove(T item)
		{
			bool rt;
			try
			{
				_lock.EnterWriteLock();
				rt = _hashSet.Remove(item);
			}
			finally
			{
                if (_lock.IsWriteLockHeld)
				{
					_lock.ExitWriteLock();
				}
			}
			return rt;
		}

		public int Count
		{
			get
			{
				int count;
				try
				{
					_lock.EnterReadLock();
					count = _hashSet.Count;
				}
				finally
				{
					if (_lock.IsReadLockHeld)
					{
						_lock.ExitReadLock();
					}
				}
				return count;
			}
		}

		public void Dispose()
		{
			if (_lock != null)
			{
				_lock.Dispose();
			}
		}
	}
}
