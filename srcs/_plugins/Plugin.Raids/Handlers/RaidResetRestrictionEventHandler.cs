using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsAPI.Data.Character;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Handlers;

public class RaidResetRestrictionEventHandler : IAsyncEventProcessor<RaidResetRestrictionEvent>
{
    private readonly IExpirableLockService _lockService;

    public RaidResetRestrictionEventHandler(IExpirableLockService lockService) => _lockService = lockService;

    public async Task HandleAsync(RaidResetRestrictionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!await _lockService.TryAddTemporaryLockAsync($"game:locks:raid-restriction:{session.PlayerEntity.Id}", DateTime.UtcNow.Date.AddDays(1)))
        {
            return;
        }

        session.PlayerEntity.RaidRestrictionDto ??= new CharacterRaidRestrictionDto();
        session.PlayerEntity.RaidRestrictionDto.LordDraco = 30;
        session.PlayerEntity.RaidRestrictionDto.Glacerus = 30;
    }
}