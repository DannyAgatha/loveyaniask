using System.Collections.Generic;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.GameEvent.Matchmaking.Filter;

public class FilterResult
{
    public FilterResult(List<IClientSession> acceptedSessions, List<IClientSession> refusedSessions)
    {
        AcceptedSessions = acceptedSessions;
        RefusedSessions = refusedSessions;
    }

    public List<IClientSession> AcceptedSessions { get; }
    public List<IClientSession> RefusedSessions { get; }
}