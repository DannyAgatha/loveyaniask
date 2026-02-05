using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families.Event;

public class FamilyChangeTitleEvent : PlayerEvent
{
    public FamilyChangeTitleEvent(string nickname, FamilyTitle title)
    {
        MemberNickname = nickname;
        Title = title;
    }

    public string MemberNickname { get; }

    public FamilyTitle Title { get; }
}