using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidListJoinEvent : PlayerEvent
{
    public RaidListJoinEvent(string nickname) => Nickname = nickname;

    public string Nickname { get; }
}