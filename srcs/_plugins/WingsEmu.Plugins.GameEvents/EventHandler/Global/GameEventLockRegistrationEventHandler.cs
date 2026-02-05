using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Core.Generics;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace WingsEmu.Plugins.GameEvents.EventHandler.Global
{
    public class GameEventLockRegistrationEventHandler : IAsyncEventProcessor<GameEventLockRegistrationEvent>
    {
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameEventRegistrationManager _gameEventRegistrationManager;
        private readonly IGameLanguageService _languageService;
        private readonly ISessionManager _sessionManager;

        public GameEventLockRegistrationEventHandler(IGameEventRegistrationManager gameEventRegistrationManager, ISessionManager sessionManager, IGameLanguageService languageService,
            IAsyncEventPipeline eventPipeline)
        {
            _gameEventRegistrationManager = gameEventRegistrationManager;
            _sessionManager = sessionManager;
            _languageService = languageService;
            _eventPipeline = eventPipeline;
        }

        public async Task HandleAsync(GameEventLockRegistrationEvent e, CancellationToken cancellation)
        {
            _gameEventRegistrationManager.RemoveGameEventRegistration(e.Type);
            
            Game18NConstString? gameEventKey = e.Type switch
            {
                GameEventType.InstantBattle => Game18NConstString.InstantCombatStarted,
                _ => null
            };

            Log.Debug("[GameEvent] Locked Registration for Event: " + e.Type);
            
            if (gameEventKey.HasValue)
            {
                _sessionManager.Broadcast(x => x.GenerateMsgiPacket(MessageType.Notification, gameEventKey.Value));
            }

            _sessionManager.Broadcast(x => x.GenerateEsfPacket(4));

            ThreadSafeHashSet<long> registeredCharacters = _gameEventRegistrationManager.GetAndRemoveCharactersByGameEventInclination(e.Type);
            if (registeredCharacters == null)
            {
                return;
            }

            var list = new List<IClientSession>();

            foreach (long character in registeredCharacters)
            {
                IClientSession session = _sessionManager.GetSessionByCharacterId(character);
                if (session == null)
                {
                    continue;
                }

                list.Add(session);
            }

            await _eventPipeline.ProcessEventAsync(new GameEventMatchmakeEvent(e.Type, list), cancellation);
        }
    }
}