using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateSpUntransformEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; set; }
}