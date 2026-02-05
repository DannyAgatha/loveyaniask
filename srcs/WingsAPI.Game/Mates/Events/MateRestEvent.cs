using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateRestEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }
    public bool Rest { get; init; }
    public bool Force { get; init; }
}