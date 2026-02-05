using System.Collections.Generic;
using WingsAPI.Data.Shops;
using WingsEmu.DTOs.Shops;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Shops;

namespace NosEmu.Plugins.BasicImplementations.Entities;

public class ShopFactory : IShopFactory
{
    private readonly List<ShopItemDTO> _emptyItemList = new();
    private readonly List<ShopSkillDTO> _emptySkillList = new();

    public ShopNpc CreateShop(ShopDTO shopDto) => new ShopNpc(shopDto.MapNpcId, (ShopNpcMenuType)shopDto.MenuType, shopDto.Name, shopDto.ShopType, shopDto.Items ?? _emptyItemList,
        shopDto.Skills ?? _emptySkillList);
}