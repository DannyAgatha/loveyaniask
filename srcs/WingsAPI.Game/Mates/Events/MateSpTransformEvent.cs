using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateSpTransformEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; set; }
}