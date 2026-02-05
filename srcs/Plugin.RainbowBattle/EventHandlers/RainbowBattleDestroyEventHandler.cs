using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleDestroyEventHandler : IAsyncEventProcessor<RainbowBattleDestroyEvent>
    {
        private readonly IMapManager _mapManager;
        private readonly IRainbowBattleManager _rainbowBattleManager;

        public RainbowBattleDestroyEventHandler(IRainbowBattleManager rainbowBattleManager, IMapManager mapManager)
        {
            _rainbowBattleManager = rainbowBattleManager;
            _mapManager = mapManager;
        }

        public async Task HandleAsync(RainbowBattleDestroyEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowBattleParty = e.RainbowBattleParty;
            _rainbowBattleManager.RemoveRainbowBattle(rainbowBattleParty);

            IMapInstance mapInstance = rainbowBattleParty.MapInstance;
            foreach (IClientSession session in mapInstance.Sessions)
            {
                await session.EmitEventAsync(new RainbowBattleLeaveEvent
                {
                    AddLeaverBuster = false
                });
            }

            _mapManager.RemoveMapInstance(mapInstance.Id);
        }
    }
}