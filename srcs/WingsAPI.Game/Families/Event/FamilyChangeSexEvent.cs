using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyChangeSexEvent : PlayerEvent
{
    public FamilyChangeSexEvent(byte gender) => Gender = gender;

    public byte Gender { get; }
}