using System.Threading.Tasks;
using Foundatio.Caching;

namespace PhoenixLib.DAL.Redis.Locks
{
    public class RedisLockService : ILockService
    {
        private readonly ICacheClient _database;

        public RedisLockService(ICacheClient database) => _database = database;

        public async Task<IScopedLock> TryAcquireScopedLock(string key) => new RedisScopedLock(key, this, await TryAcquireLock(key));

        public async Task<bool> IsLocked(string key) => await _database.ExistsAsync(key);

        public Task<bool> TryAcquireLock(string key) => _database.AddAsync(key, 1.ToString());

        public Task<bool> TryFreeLock(string key) => _database.RemoveAsync(key);
    }
}