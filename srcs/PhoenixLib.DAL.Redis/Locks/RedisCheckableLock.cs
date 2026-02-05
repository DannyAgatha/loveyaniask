using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace PhoenixLib.DAL.Redis.Locks
{
    public class RedisCheckableLock : IExpirableLockService
    {
        private readonly IDatabase _database;

        public RedisCheckableLock(IConnectionMultiplexer multiplexer) => _database = multiplexer.GetDatabase(0);

        public async Task<bool> TryAddTemporaryLockAsync(string key, DateTime dateTime)
        {
            bool value = await _database.KeyExistsAsync(key);
            if (value)
            {
                // key already exists
                return false;
            }

            long tmp = await _database.StringIncrementAsync(key);
            TimeSpan expireIn = dateTime - DateTime.UtcNow;
            await _database.KeyExpireAsync(key, expireIn);

            return true;
        }

        public async Task<(bool, int newValue)> TryIncrementTemporaryLockCounter(string key, int maxValue, DateTime expirationDate)
        {
            RedisValueWithExpiry value = await _database.StringGetWithExpiryAsync(key);
            if (!value.Value.HasValue)
            {
                int newValue = (int)await _database.StringIncrementAsync(key);
                await _database.KeyExpireAsync(key, expirationDate);
                return (true, newValue);
            }

            int counter = int.Parse(value.Value);
            if (counter >= maxValue)
            {
                // should not go above maxValue
                return (false, counter);
            }

            int tmp = (int)await _database.StringIncrementAsync(key);
            await _database.KeyExpireAsync(key, expirationDate);

            return (true, tmp);
        }

        public async Task<(bool, int newValue)> TryGetTemporaryCounterValue(string key)
        {
            RedisValueWithExpiry value = await _database.StringGetWithExpiryAsync(key);
            if (!value.Value.HasValue)
            {
                return (false, 0);
            }

            int counter = int.Parse(value.Value);
            return (true, counter);
        }

        public async Task<bool> TryRemoveTemporaryLock(string key) => await _database.KeyDeleteAsync(key);

        public async Task<bool> ExistsTemporaryLock(string key) => await _database.KeyExistsAsync(key);
        
        public async Task<DateTime?> GetLockExpirationAsync(string key)
        {
            TimeSpan? ttl = await _database.KeyTimeToLiveAsync(key);

            return ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : DateTime.UtcNow.Date.AddMinutes(1);
        }
    }
}