using System;

namespace PhoenixLib.DAL.Redis.Locks
{
    public interface IScopedLock : IAsyncDisposable
    {
        bool IsAcquired { get; }
    }
}