using System.Threading.Tasks;

namespace PhoenixLib.DAL.Redis.Locks
{
    public sealed class RedisScopedLock : IScopedLock
    {
        private readonly string _lockKey;
        private readonly ILockService _lockService;

        public RedisScopedLock(string lockKey, ILockService lockService, bool isLockAcquired)
        {
            _lockKey = lockKey;
            _lockService = lockService;
            IsAcquired = isLockAcquired;
        }

        public async ValueTask DisposeAsync()
        {
            if (!IsAcquired)
            {
                return;
            }

            await _lockService.TryFreeLock(_lockKey);
        }

        public bool IsAcquired { get; }
    }
}