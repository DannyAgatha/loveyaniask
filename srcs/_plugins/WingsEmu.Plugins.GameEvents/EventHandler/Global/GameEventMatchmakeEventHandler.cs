using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.GameEvent.Event;
using WingsEmu.Game.GameEvent.Matchmaking;
using WingsEmu.Game.GameEvent.Matchmaking.Filter;
using WingsEmu.Game.GameEvent.Matchmaking.Result;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Event.Global;
using WingsEmu.Plugins.GameEvents.Matchmaking.Filter;

namespace WingsEmu.Plugins.GameEvents.EventHandler.Global
{
    public class GameEventMatchmakeEventHandler : IAsyncEventProcessor<GameEventMatchmakeEvent>
    {
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameLanguageService _languageService;
        private readonly IMatchmaking _matchmaking;

        public GameEventMatchmakeEventHandler(IAsyncEventPipeline eventPipeline, IMatchmaking matchmaking, IGameLanguageService languageService)
        {
            _eventPipeline = eventPipeline;
            _matchmaking = matchmaking;
            _languageService = languageService;
        }

        public async Task HandleAsync(GameEventMatchmakeEvent e, CancellationToken cancellation)
        {
            Log.Debug("[GameEvent] Processing matchmaking for Event: " + e.Type);
            FilterResult filterResult = _matchmaking.Filter(e.Sessions, new InBaseMapFilter());
            TellToRefusedSessions(filterResult.RefusedSessions);

            IMatchmakingResult matchmakingResult = _matchmaking.Matchmake(filterResult.AcceptedSessions, e.Type);

            if (matchmakingResult == null)
            {
                return;
            }

            await ProcessMatchmakingResult(matchmakingResult);
        }

        private void TellToRefusedSessions(IEnumerable<IClientSession> refusedSessions)
        {
            foreach (IClientSession session in refusedSessions)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.GAMEEVENT_MESSAGE_NOT_IN_GENERAL_MAP, session.UserLanguage), MsgMessageType.SmallMiddle);
            }
        }

        private void TellToRefusedSessions(Dictionary<Game18NConstString, List<IClientSession>> refusedSessions)
        {
            foreach ((Game18NConstString gameDialogKey, List<IClientSession> clientSessions) in refusedSessions)
            {
                foreach (IClientSession session in clientSessions)
                {
                    session.SendMsgi(MessageType.Center, gameDialogKey);
                }
            }
        }

        private async Task ProcessMatchmakingResult(IMatchmakingResult matchmakingResult)
        {
            foreach (Tuple<IGameEventConfiguration, List<IClientSession>> pair in matchmakingResult.Sessions)
            {
                await ProcessGameEventStart(pair);
            }

            TellToRefusedSessions(matchmakingResult.RefusedSessions);
        }

        private async Task ProcessGameEventStart(Tuple<IGameEventConfiguration, List<IClientSession>> pair)
        {
            (IGameEventConfiguration gameEventConfiguration, List<IClientSession> clientSessions) = pair;

            await _eventPipeline.ProcessEventAsync(new GameEventInstanceStartEvent(clientSessions, gameEventConfiguration));
        }
    }
}