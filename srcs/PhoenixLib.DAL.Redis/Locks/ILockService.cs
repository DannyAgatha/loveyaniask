using System.Threading.Tasks;

namespace PhoenixLib.DAL.Redis.Locks
{
    public interface ILockService
    {
        Task<IScopedLock> TryAcquireScopedLock(string key);

        Task<bool> IsLocked(string key);

        Task<bool> TryAcquireLock(string key);
        Task<bool> TryFreeLock(string key);
    }
}