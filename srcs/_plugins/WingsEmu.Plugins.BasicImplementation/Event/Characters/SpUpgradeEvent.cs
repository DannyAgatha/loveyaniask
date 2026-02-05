using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SpUpgradeEvent : PlayerEvent
{
    public SpUpgradeEvent(UpgradeProtection upgradeProtection, InventoryItem inventoryItem, bool isFree = false, bool isAutoUpgrade = false, bool isPremium = false)
    {
        UpgradeProtection = upgradeProtection;
        InventoryItem = inventoryItem;
        IsFree = isFree;
        IsAutoUpgrade = isAutoUpgrade;
        IsPremium = isPremium;
    }

    public UpgradeProtection UpgradeProtection { get; }
    public InventoryItem InventoryItem { get; }
    public bool IsFree { get; }
    public bool IsAutoUpgrade { get; }
    
    public bool IsPremium { get; }
}