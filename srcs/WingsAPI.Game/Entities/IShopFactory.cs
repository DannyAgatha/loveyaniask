using WingsAPI.Data.Shops;
using WingsEmu.Game.Shops;

namespace WingsEmu.Game.Entities;

public interface IShopFactory
{
    ShopNpc CreateShop(ShopDTO shopDto);
}