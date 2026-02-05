using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Portals;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceGroupTryJoinEvent : PlayerEvent
{
    public ITimeSpacePortalEntity PortalEntity { get; init; }
    public long CharacterId { get; init; }
}