using System.Collections.Generic;
using WingsEmu.Game.GameEvent.Matchmaking.Filter;
using WingsEmu.Game.GameEvent.Matchmaking.Result;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.GameEvent.Matchmaking;

public interface IMatchmaking
{
    IMatchmakingResult Matchmake(List<IClientSession> sessions, GameEventType type);
    FilterResult Filter(List<IClientSession> sessions, params IMatchmakingFilter[] filters);
}