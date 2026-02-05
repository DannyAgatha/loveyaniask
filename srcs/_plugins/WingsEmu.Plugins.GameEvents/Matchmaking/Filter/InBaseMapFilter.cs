using WingsEmu.DTOs.Maps;
using WingsEmu.Game.GameEvent.Matchmaking.Filter;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Matchmaking.Filter
{
    public class InBaseMapFilter : IMatchmakingFilter
    {
        public bool IsAccepted(IClientSession session) => session.PlayerEntity.MapInstance != null && session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP);
    }
}