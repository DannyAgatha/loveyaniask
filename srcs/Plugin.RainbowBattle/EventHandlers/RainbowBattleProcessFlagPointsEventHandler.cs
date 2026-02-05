using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleProcessFlagPointsEventHandler : IAsyncEventProcessor<RainbowBattleProcessFlagPointsEvent>
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;

        public RainbowBattleProcessFlagPointsEventHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

        public async Task HandleAsync(RainbowBattleProcessFlagPointsEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowParty = e.RainbowBattleParty;

            await _asyncEventPipeline.ProcessEventAsync(new RainbowBattleRefreshScoreEvent
            {
                RainbowBattleParty = rainbowParty
            });
        }
    }
}