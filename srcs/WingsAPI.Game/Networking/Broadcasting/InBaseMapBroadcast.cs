using WingsEmu.DTOs.Maps;

namespace WingsEmu.Game.Networking.Broadcasting;

public class InBaseMapBroadcast : IBroadcastRule
{
    public bool Match(IClientSession session) => session.CurrentMapInstance != null && session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP);
}