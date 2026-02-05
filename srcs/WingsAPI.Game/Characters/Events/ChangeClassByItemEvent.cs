using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Characters.Events;

public class ChangeClassByItemEvent : PlayerEvent
{
    public ClassType NewClassType { get; set; }
    
    public InventoryItem ItemInstance { get; set; }
    
    public bool Confirmation { get; set; }
}