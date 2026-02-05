// NosEmu
// 


namespace WingsEmu.DTOs.ServerDatas;

public class ItemBoxItemDto
{
    public short Probability { get; set; }
    public short MinimumOriginalItemRare { get; set; }
    public short MaximumOriginalItemRare { get; set; }
    public short ItemGeneratedAmount { get; set; }
    public int ItemGeneratedVNum { get; set; }
    
    public sbyte ItemGeneratedRarity { get; set; }
    public bool ItemGeneratedRandomRarity { get; set; }
    public byte ItemGeneratedUpgrade { get; set; }
}