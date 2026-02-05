// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Shops;

namespace WingsEmu.Game.Shops;

public class ShopNpc
{
    public ShopNpc(int mapNpcId, ShopNpcMenuType menuType, string name, byte shopType, IReadOnlyList<ShopItemDTO> shopItems, IReadOnlyList<ShopSkillDTO> shopSkills)
    {
        MapNpcId = mapNpcId;
        MenuType = menuType;
        Name = name;
        ShopType = shopType;
        ShopItems = shopItems;
        ShopSkills = shopSkills;
    }

    public int MapNpcId { get; }
    public ShopNpcMenuType MenuType { get; }
    public string Name { get; }
    public byte ShopType { get; }
    public IReadOnlyList<ShopItemDTO> ShopItems { get; }
    public IReadOnlyList<ShopSkillDTO> ShopSkills { get; }
}