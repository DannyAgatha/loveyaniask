using System;
using System.Collections.Generic;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.GameEvent.Matchmaking.Result;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.GameEvents.Matchmaking.Result
{
    public class InstantBattleMatchmakingResult : IMatchmakingResult
    {
        public InstantBattleMatchmakingResult(List<Tuple<IGameEventConfiguration, List<IClientSession>>> sessions, Dictionary<Game18NConstString, List<IClientSession>> refusedSessions)
        {
            Sessions = sessions;
            RefusedSessions = refusedSessions;
        }

        public List<Tuple<IGameEventConfiguration, List<IClientSession>>> Sessions { get; }

        public Dictionary<Game18NConstString, List<IClientSession>> RefusedSessions { get; }
    }
}