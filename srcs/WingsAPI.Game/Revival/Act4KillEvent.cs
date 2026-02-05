using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Revival;

public class Act4KillEvent : PlayerEvent
{
    public long TargetId { get; init; }
}