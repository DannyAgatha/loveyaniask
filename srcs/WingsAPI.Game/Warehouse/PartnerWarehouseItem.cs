using WingsEmu.DTOs.Inventory;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Warehouse;

public class PartnerWarehouseItem : PartnerWarehouseItemDto
{
    public GameItemInstance ItemInstance { get; set; }
}