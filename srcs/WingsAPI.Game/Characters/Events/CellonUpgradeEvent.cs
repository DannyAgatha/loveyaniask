using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Characters.Events;

public class CellonUpgradeEvent : PlayerEvent
{
    public CellonUpgradeEvent(InventoryItem cellon, GameItemInstance upgradableItem)
    {
        Cellon = cellon;
        UpgradableItem = upgradableItem;
    }

    public InventoryItem Cellon { get; }
    public GameItemInstance UpgradableItem { get; }
}