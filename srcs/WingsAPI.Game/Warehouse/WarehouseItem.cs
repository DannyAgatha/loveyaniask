using WingsAPI.Data.Account;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Warehouse;

public class WarehouseItem : AccountWarehouseItemDto
{
    public GameItemInstance ItemInstance { get; set; }
}