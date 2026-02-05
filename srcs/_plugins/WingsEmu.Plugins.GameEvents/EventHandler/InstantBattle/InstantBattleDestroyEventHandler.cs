using PhoenixLib.Events;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.GameEvents.Event.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.EventHandler.InstantBattle
{
    public class InstantBattleDestroyEventHandler : IAsyncEventProcessor<InstantBattleDestroyEvent>
    {
        private readonly IGameEventInstanceManager _gameEventInstanceManager;
        private readonly IMapManager _mapManager;

        public InstantBattleDestroyEventHandler(IMapManager mapManager, IGameEventInstanceManager gameEventInstanceManager)
        {
            _mapManager = mapManager;
            _gameEventInstanceManager = gameEventInstanceManager;
        }

        public async Task HandleAsync(InstantBattleDestroyEvent e, CancellationToken cancellation)
        {
            foreach (IClientSession session in e.Instance.MapInstance.Sessions.ToList())
            {
                session.ChangeToLastBaseMap();
            }

            _gameEventInstanceManager.RemoveGameEvent(e.Instance);
            _mapManager.RemoveMapInstance(e.Instance.MapInstance.Id);
        }
    }
}