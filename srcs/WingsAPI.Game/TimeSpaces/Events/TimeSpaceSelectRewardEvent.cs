using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceSelectRewardEvent : PlayerEvent
{
    public bool SendRepayPacket { get; init; }
}