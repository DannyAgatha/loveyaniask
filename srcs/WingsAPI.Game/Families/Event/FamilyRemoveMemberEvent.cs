using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyRemoveMemberEvent : PlayerEvent
{
    public FamilyRemoveMemberEvent(string nickname) => Nickname = nickname;

    public string Nickname { get; }
}