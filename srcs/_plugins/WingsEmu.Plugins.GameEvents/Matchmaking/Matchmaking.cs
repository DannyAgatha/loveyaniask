using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Matchmaking;
using WingsEmu.Game.GameEvent.Matchmaking.Filter;
using WingsEmu.Game.GameEvent.Matchmaking.Matchmaker;
using WingsEmu.Game.GameEvent.Matchmaking.Result;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Matchmaking
{
    public class Matchmaking : IMatchmaking
    {
        private readonly Dictionary<GameEventType, IMatchmaker> _matchmakers;

        public Matchmaking(Dictionary<GameEventType, IMatchmaker> matchmakers) => _matchmakers = matchmakers;

        public IMatchmakingResult Matchmake(List<IClientSession> sessions, GameEventType type) => _matchmakers[type].Matchmake(sessions);

        public FilterResult Filter(List<IClientSession> sessions, params IMatchmakingFilter[] filters)
        {
            var accepted = sessions.Where(session => filters.All(x => x.IsAccepted(session))).ToList();

            return new FilterResult(accepted, sessions.Except(accepted).ToList());
        }
    }
}