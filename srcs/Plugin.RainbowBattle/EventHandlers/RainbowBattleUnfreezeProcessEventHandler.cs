using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleUnfreezeProcessEventHandler : IAsyncEventProcessor<RainbowBattleUnfreezeProcessEvent>
    {
        public async Task HandleAsync(RainbowBattleUnfreezeProcessEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowParty = e.RainbowBattleParty;

            DateTime now = DateTime.UtcNow;
            foreach (IClientSession session in rainbowParty.MapInstance.Sessions)
            {
                if (!session.PlayerEntity.RainbowBattleComponent.IsFrozen)
                {
                    continue;
                }

                if (!session.PlayerEntity.RainbowBattleComponent.FrozenTime.HasValue)
                {
                    // isFrozen and Frozen time is null? Unfreeze player
                    await session.EmitEventAsync(new RainbowBattleUnfreezeEvent());
                    continue;
                }

                if (session.PlayerEntity.RainbowBattleComponent.FrozenTime > now)
                {
                    continue;
                }

                await session.EmitEventAsync(new RainbowBattleUnfreezeEvent());
            }
        }
    }
}