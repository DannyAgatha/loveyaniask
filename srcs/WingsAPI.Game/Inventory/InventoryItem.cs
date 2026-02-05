using WingsEmu.DTOs.Inventory;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Inventory;

public class InventoryItem : CharacterInventoryItemDto
{
    public GameItemInstance ItemInstance { get; set; }
}