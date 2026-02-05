using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Event;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.GameEvents.EventHandler.InstantBattle
{
    public class GameEventInstanceStartEventInstantBattleHandler : IAsyncEventProcessor<GameEventInstanceStartEvent>
    {
        private readonly IGameEventInstanceManager _gameEventInstanceManager;
        private readonly IMapManager _mapManager;

        public GameEventInstanceStartEventInstantBattleHandler(IGameEventInstanceManager gameEventInstanceManager, IMapManager mapManager)
        {
            _gameEventInstanceManager = gameEventInstanceManager;
            _mapManager = mapManager;
        }

        public async Task HandleAsync(GameEventInstanceStartEvent e, CancellationToken cancellation)
        {
            if (e.GameEventConfiguration.GameEventType != GameEventType.InstantBattle)
            {
                return;
            }

            IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(e.GameEventConfiguration.MapId, e.GameEventConfiguration.MapInstanceType);
            if (mapInstance == null)
            {
                Log.Warn($"[INSTANT_BATTLE_GAME_EVENT] Couldn't generate the desired map with mapId: '{e.GameEventConfiguration.MapId.ToString()}'");
            }

            var instance = new InstantBattleInstance(mapInstance, (InstantBattleConfiguration)e.GameEventConfiguration);

            Log.Debug($"[GameEvent] Teleporting {e.Sessions.Count().ToString()} users");
            foreach (IClientSession session in e.Sessions)
            {
                await _mapManager.TeleportOnRandomPlaceInMapAsync(session, instance.MapInstance);
            }

            _gameEventInstanceManager.AddGameEvent(instance);
        }
    }
}