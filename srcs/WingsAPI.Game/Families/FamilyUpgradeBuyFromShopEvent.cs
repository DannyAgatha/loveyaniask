using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families;

public class FamilyUpgradeBuyFromShopEvent : PlayerEvent
{
    public long NpcId { get; set; }
    public short Slot { get; set; }
}