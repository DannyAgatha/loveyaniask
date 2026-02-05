using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Groups.Events;

public class JoinToGroupEvent : PlayerEvent
{
    public JoinToGroupEvent(PlayerGroup playerGroup) => PlayerGroup = playerGroup;

    public PlayerGroup PlayerGroup { get; }
}