// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Shops;

namespace WingsAPI.Data.Shops;

public class ShopDTO
{
    public int MapNpcId { get; set; }

    public byte MenuType { get; set; }

    public string Name { get; set; }

    public byte ShopType { get; set; }

    public List<ShopSkillDTO> Skills { get; set; }
    public List<ShopItemDTO> Items { get; set; }
}