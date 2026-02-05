using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpacePickUpItemEvent : PlayerEvent
{
    public TimeSpaceMapItem TimeSpaceMapItem { get; init; }
    public IMateEntity MateEntity { get; init; }
}