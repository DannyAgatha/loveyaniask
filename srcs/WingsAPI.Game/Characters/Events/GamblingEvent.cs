using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class GamblingEvent : PlayerEvent
{
    public GamblingEvent(InventoryItem item, InventoryItem amulet, RarifyMode mode, RarifyProtection protection)
    {
        Item = item;
        Amulet = amulet;
        Mode = mode;
        Protection = protection;
    }

    public RarifyProtection Protection { get; }
    public RarifyMode Mode { get; }
    public InventoryItem Item { get; }
    public InventoryItem Amulet { get; }
}