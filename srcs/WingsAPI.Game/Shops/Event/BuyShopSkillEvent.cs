using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class BuyShopSkillEvent : PlayerEvent
{
    public long OwnerId { get; set; }
    public short Slot { get; set; }
    public bool Accept { get; set; }
}