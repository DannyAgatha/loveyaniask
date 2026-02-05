using System;
using System.Threading.Tasks;

namespace PhoenixLib.DAL.Redis.Locks
{
    public interface IExpirableLockService
    {
        /// <summary>
        ///     Sets a key without increment with a defined expiration date
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expirationDate"></param>
        /// <returns>true if the key already exist</returns>
        Task<bool> TryAddTemporaryLockAsync(string key, DateTime expirationDate);

        /// <summary>
        ///     Sets an incremental key with a defined maxValue, expires on the specified expirationDate
        /// </summary>
        /// <param name="key"></param>
        /// <param name="maxValue"></param>
        /// <param name="expirationDate"></param>
        /// <returns></returns>
        Task<(bool, int newValue)> TryIncrementTemporaryLockCounter(string key, int maxValue, DateTime expirationDate);

        /// <summary>
        ///     Try getting the current value of the counter
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<(bool, int newValue)> TryGetTemporaryCounterValue(string key);

        /// <summary>
        ///     Removes a key if exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if the key exists and got removed</returns>
        Task<bool> TryRemoveTemporaryLock(string key);

        /// <summary>
        ///     Checks if a key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if the key exists</returns>
        Task<bool> ExistsTemporaryLock(string key);
        
        /// <summary>
        /// Gets the expiration date of the lock.
        /// </summary>
        /// <param name="key">The key of the lock.</param>
        /// <returns>The expiration date of the lock, or null if the lock doesn't exist.</returns>
        Task<DateTime?> GetLockExpirationAsync(string key);
    }
}