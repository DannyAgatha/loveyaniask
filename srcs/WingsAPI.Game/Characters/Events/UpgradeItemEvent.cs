using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class UpgradeItemEvent : PlayerEvent
{
    public InventoryItem Inv { get; set; }
    public UpgradeMode Mode { get; set; }
    public UpgradeProtection Protection { get; set; }
    public FixedUpMode HasAmulet { get; set; } = FixedUpMode.None;
    public bool IsAutoUpgrade { get; set; } = false;
    public bool IsPremium { get; set; } = false;
}