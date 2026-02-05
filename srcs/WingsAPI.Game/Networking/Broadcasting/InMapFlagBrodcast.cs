using WingsEmu.DTOs.Maps;

namespace WingsEmu.Game.Networking.Broadcasting
{
    public class InMapFlagBrodcast : IBroadcastRule
    {
        private readonly MapFlags _mapFlags;
        public InMapFlagBrodcast(MapFlags mapFlags) => _mapFlags = mapFlags;

        public bool Match(IClientSession session) => session.CurrentMapInstance != null && session.CurrentMapInstance.HasMapFlag(_mapFlags);
    }
}