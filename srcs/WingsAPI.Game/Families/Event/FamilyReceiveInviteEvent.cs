using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyReceiveInviteEvent : PlayerEvent
{
    public FamilyReceiveInviteEvent(string familyName, long senderCharacterId, long familyId)
    {
        FamilyName = familyName;
        SenderCharacterId = senderCharacterId;
        FamilyId = familyId;
    }

    public long SenderCharacterId { get; }

    public long FamilyId { get; }

    public string FamilyName { get; }
}