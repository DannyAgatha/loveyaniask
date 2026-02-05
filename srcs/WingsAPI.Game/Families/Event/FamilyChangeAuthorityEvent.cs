using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families.Event;

public class FamilyChangeAuthorityEvent : PlayerEvent
{
    public FamilyChangeAuthorityEvent(FamilyAuthority familyAuthority, long memberId, byte confirmed, string characterName = null)
    {
        MemberId = memberId;
        Confirmed = confirmed;
        CharacterName = characterName;
        FamilyAuthority = familyAuthority;
    }

    public FamilyAuthority FamilyAuthority { get; }

    public long MemberId { get; }

    public byte Confirmed { get; }

    public string CharacterName { get; }
}