// NosEmu
// 


namespace WingsEmu.DTOs.Shops;

public class ShopItemDTO
{
    public short Slot { get; set; }

    public byte Color { get; set; }

    public int ItemVNum { get; set; }

    public short Rare { get; set; }

    public byte Type { get; set; }

    public byte Upgrade { get; set; }

    public int? Price { get; set; }
}