using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class PlayerRestEvent : PlayerEvent
{
    public bool RestTeamMemberMates { get; set; }
    public bool Force { get; init; }
}