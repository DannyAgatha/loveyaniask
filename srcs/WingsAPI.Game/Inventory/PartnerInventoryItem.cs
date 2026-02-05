using WingsEmu.DTOs.Inventory;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Inventory;

public class PartnerInventoryItem : CharacterPartnerInventoryItemDto
{
    public GameItemInstance ItemInstance { get; set; }
}