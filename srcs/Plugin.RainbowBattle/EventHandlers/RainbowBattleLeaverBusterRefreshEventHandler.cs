using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsAPI.Data.Character;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleLeaverBusterRefreshEventHandler : IAsyncEventProcessor<RainbowBattleLeaverBusterRefreshEvent>
    {
        private readonly IExpirableLockService _lock;

        public RainbowBattleLeaverBusterRefreshEventHandler(IExpirableLockService @lock) => _lock = @lock;

        public async Task HandleAsync(RainbowBattleLeaverBusterRefreshEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            DateTime nextMonth = DateTime.UtcNow.Date.AddMonths(1).AddDays(-DateTime.UtcNow.Date.Day + 1);
            bool canRefresh = await _lock.TryAddTemporaryLockAsync($"game:locks:rainbow-battle-leaver-buster:{session.PlayerEntity.Id}", nextMonth);

            if (!e.Force && !canRefresh)
            {
                return;
            }

            session.PlayerEntity.RainbowBattleLeaverBusterDto ??= new RainbowBattleLeaverBusterDto();
            session.PlayerEntity.RainbowBattleLeaverBusterDto.Exits = 0;
        }
    }
}