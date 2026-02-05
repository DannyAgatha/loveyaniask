using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Rewards.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.DailyRewards;

public class RewardResetRestrictionEventHandler : IAsyncEventProcessor<RewardResetRestrictionEvent>
{
    private readonly IExpirableLockService _lockService;

    public RewardResetRestrictionEventHandler(IExpirableLockService lockService) => _lockService = lockService;

    public async Task HandleAsync(RewardResetRestrictionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!await _lockService.TryAddTemporaryLockAsync($"game:locks:dailyreward-restriction:{session.IpAddress}", DateTime.UtcNow.Date.AddDays(1)))
        {
            return;
        }

        session.PlayerEntity.DailyRewardDto.Clear();
    }
}