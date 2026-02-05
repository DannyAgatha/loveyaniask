using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateJoinInMinilandEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }
}