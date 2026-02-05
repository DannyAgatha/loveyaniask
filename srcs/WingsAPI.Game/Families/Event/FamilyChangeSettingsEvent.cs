using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families.Event;

public class FamilyChangeSettingsEvent : PlayerEvent
{
    public FamilyAuthority Authority { get; set; }
    public FamilyActionType FamilyActionType { get; set; }
    public byte Value { get; set; }
}