// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Shops;

public class ShopPlayerItem
{
    public InventoryType InventoryType { get; set; }

    public short InventorySlot { get; set; }

    public long PricePerUnit { get; set; }

    public int SellAmount { get; set; }

    public short ShopSlot { get; set; }
}