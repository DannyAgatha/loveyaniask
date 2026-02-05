using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopSkillSoldEvent : PlayerEvent
{
    public int SkillVnum { get; init; }
}