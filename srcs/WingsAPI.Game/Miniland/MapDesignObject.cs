// NosEmu
// 


using WingsAPI.Data.Miniland;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Miniland;

public class MapDesignObject : CharacterMinilandObjectDto
{
    public long CharacterId { get; set; }
    public InventoryItem InventoryItem { get; set; }
}