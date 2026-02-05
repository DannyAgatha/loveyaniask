using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateJoinTeamEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }

    public bool IsOnCharacterEnter { get; init; }

    public bool IsNewCreated { get; init; }
}