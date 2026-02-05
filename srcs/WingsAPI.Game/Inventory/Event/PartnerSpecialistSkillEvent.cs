using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class PartnerSpecialistSkillEvent : PlayerEvent
{
    public byte PartnerSlot { get; init; }
    public byte SkillSlot { get; init; }
    public bool Roll { get; init; }
}