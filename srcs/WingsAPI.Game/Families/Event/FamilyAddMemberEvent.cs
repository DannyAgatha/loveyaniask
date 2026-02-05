using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families.Event;

public class FamilyAddMemberEvent : PlayerEvent
{
    public FamilyAddMemberEvent(long familyIdToJoin, long senderId, FamilyAuthority familyAuthority = FamilyAuthority.Member)
    {
        FamilyIdToJoin = familyIdToJoin;
        FamilyAuthority = familyAuthority;
        SenderId = senderId;
    }

    public long SenderId { get; }

    public long FamilyIdToJoin { get; }

    public FamilyAuthority FamilyAuthority { get; }
}