using WingsEmu.Game.Networking;

namespace WingsEmu.Game.GameEvent.Matchmaking.Filter;

public interface IMatchmakingFilter
{
    bool IsAccepted(IClientSession session);
}