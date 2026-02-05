using System;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using WingsEmu.Game.Compliments;

namespace NosEmu.Plugins.BasicImplementations.Compliments;

public class ComplimentsManager : IComplimentsManager
{
    private readonly IExpirableLockService _expirableLock;

    public ComplimentsManager(IExpirableLockService expirableLock) => _expirableLock = expirableLock;

    public async Task<bool> CanRefresh(long characterId)
    {
        DateTime nextMonth = DateTime.UtcNow.Date.AddMonths(1).AddDays(-DateTime.UtcNow.Date.Day + 1);
        return await _expirableLock.TryAddTemporaryLockAsync($"game:locks:compliments-monthly-refresh:{characterId}", nextMonth);
    }

    public async Task<bool> CanCompliment(long accountId) => await _expirableLock.TryAddTemporaryLockAsync($"game:locks:compliments-usage:{accountId}", DateTime.UtcNow.Date.AddDays(1));
}