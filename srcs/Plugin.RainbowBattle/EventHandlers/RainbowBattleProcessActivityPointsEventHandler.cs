using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleProcessActivityPointsEventHandler : IAsyncEventProcessor<RainbowBattleProcessActivityPointsEvent>
    {
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

        public RainbowBattleProcessActivityPointsEventHandler(RainbowBattleConfiguration rainbowBattleConfiguration) => _rainbowBattleConfiguration = rainbowBattleConfiguration;

        public async Task HandleAsync(RainbowBattleProcessActivityPointsEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowBattle = e.RainbowBattleParty;

            if (rainbowBattle?.MapInstance == null)
            {
                return;
            }

            DateTime now = DateTime.UtcNow;
            short pointsToAdd = _rainbowBattleConfiguration.WalkingActivityPoints;
            foreach (IClientSession session in rainbowBattle.MapInstance.Sessions)
            {
                if (!session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
                {
                    continue;
                }

                if (session.PlayerEntity.LastWalk?.WalkTimeStart.AddSeconds(59) < now)
                {
                    continue;
                }

                session.PlayerEntity.RainbowBattleComponent.ActivityPoints += pointsToAdd;
            }
        }
    }
}