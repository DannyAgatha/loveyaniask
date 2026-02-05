using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopSkillBoughtEvent : PlayerEvent
{
    public short SkillVnum { get; init; }
}