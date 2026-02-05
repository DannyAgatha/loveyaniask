using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateRemoveEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }
}