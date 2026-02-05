using System.Collections.Generic;
using WingsEmu.Game.GameEvent.Matchmaking.Result;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.GameEvent.Matchmaking.Matchmaker;

public interface IMatchmaker
{
    IMatchmakingResult Matchmake(List<IClientSession> sessions);
}