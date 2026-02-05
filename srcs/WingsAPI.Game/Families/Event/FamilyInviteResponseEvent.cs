using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyInviteResponseEvent : PlayerEvent
{
    public FamilyInviteResponseEvent(FamilyJoinType familyJoinType, long senderCharacterId)
    {
        SenderCharacterId = senderCharacterId;
        FamilyJoinType = familyJoinType;
    }

    public FamilyJoinType FamilyJoinType { get; }

    public long SenderCharacterId { get; }
}