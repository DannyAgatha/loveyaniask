using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateLeaveTeamEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }
}