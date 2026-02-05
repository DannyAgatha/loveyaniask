using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace WingsEmu.Plugins.GameEvents.EventHandler.Global
{
    public class GameEventUnlockRegistrationEventHandler : IAsyncEventProcessor<GameEventUnlockRegistrationEvent>
    {
        private static readonly TimeSpan RegistrationTime = TimeSpan.FromSeconds(15);
        private readonly IGameEventRegistrationManager _gameEventRegistrationManager;
        private readonly IGlobalInstantBattleConfiguration _instantBattleConfiguration;
        private readonly ISessionManager _sessionManager;

        public GameEventUnlockRegistrationEventHandler(ISessionManager sessionManager, IGameEventRegistrationManager gameEventRegistrationManager,
            IGlobalInstantBattleConfiguration instantBattleConfiguration)
        {
            _sessionManager = sessionManager;
            _gameEventRegistrationManager = gameEventRegistrationManager;
            _instantBattleConfiguration = instantBattleConfiguration;
        }

        public async Task HandleAsync(GameEventUnlockRegistrationEvent e, CancellationToken cancellation)
        {
            DateTime currentTime = DateTime.UtcNow;
            if (!_gameEventRegistrationManager.AddGameEventRegistration(e.Type, currentTime, currentTime + RegistrationTime))
            {
                Log.Debug($"[GameEvent] Already Unlocked Registration (cancelling re-unlocking event) for Event: {e.Type.ToString()}");
                return;
            }

            Log.Debug($"[GameEvent] Unlocked Registration for Event: {e.Type.ToString()}");

            IGlobalGameEventConfiguration gameEventConfiguration;
            switch (e.Type)
            {
                case GameEventType.InstantBattle:
                    gameEventConfiguration = _instantBattleConfiguration;
                    break;
                default:
                    Log.Debug($"[GameEvent] GameEventConfiguration not defined for: {e.Type.ToString()} | In: {GetType().Name}");
                    return;
            }

            _sessionManager.BroadcastGameEventAsk(e.Type, gameEventConfiguration);
        }
    }
}