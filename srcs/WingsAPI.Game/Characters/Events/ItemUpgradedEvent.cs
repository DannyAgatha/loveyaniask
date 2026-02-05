using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class ItemUpgradedEvent : PlayerEvent
{
    public ItemInstanceDTO Item { get; init; }
    public UpgradeMode Mode { get; init; }
    public UpgradeProtection Protection { get; init; }
    public bool HasAmulet { get; init; }
    public short OriginalUpgrade { get; init; }
    public UpgradeResult Result { get; init; }
    public long TotalPrice { get; init; }
}