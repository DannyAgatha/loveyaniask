using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act7.CarvedRunes;

public class UpgradeArmorCarvedRuneEvent : PlayerEvent
{
    public UpgradeArmorCarvedRuneEvent(CarvedRunesUpgradeProtection upgradeProtection, InventoryItem inventoryItem)
    {
        UpgradeProtection = upgradeProtection;
        InventoryItem = inventoryItem;
    }

    public CarvedRunesUpgradeProtection UpgradeProtection { get; }
    public InventoryItem InventoryItem { get; }
}
