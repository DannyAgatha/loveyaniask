using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateSummonEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }
}