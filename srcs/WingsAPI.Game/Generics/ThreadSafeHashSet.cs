using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WingsEmu.Core.Generics;

public sealed class ThreadSafeHashSet<T> : IEnumerable<T>, IDisposable
{
    private readonly HashSet<T> _hashSet = new();
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Count;
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                {
                    _lock.ExitReadLock();
                }
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator<T> GetEnumerator()
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet.GetEnumerator();
        }
        finally
        {
            if (_lock.IsReadLockHeld)
            {
                _lock.ExitReadLock();
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public void UnionWith(IEnumerable<T> item)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.UnionWith(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
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
        _lock.EnterReadLock();
        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            if (_lock.IsReadLockHeld)
            {
                _lock.ExitReadLock();
            }
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
            {
                _lock.ExitWriteLock();
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock?.Dispose();
        }
    }

    ~ThreadSafeHashSet() => Dispose(false);
}