using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act7.CarvedRunes;

public class UpgradeWeaponCarvedRuneEvent : PlayerEvent
{
    public UpgradeWeaponCarvedRuneEvent(CarvedRunesUpgradeProtection upgradeProtection, InventoryItem inventoryItem)
    {
        UpgradeProtection = upgradeProtection;
        InventoryItem = inventoryItem;
    }

    public CarvedRunesUpgradeProtection UpgradeProtection { get; }
    public InventoryItem InventoryItem { get; }
}
