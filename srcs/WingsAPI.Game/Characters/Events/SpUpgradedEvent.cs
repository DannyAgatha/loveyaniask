using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters.Events;

public class SpUpgradedEvent : PlayerEvent
{
    public ItemInstanceDTO Sp { get; init; }
    public UpgradeMode UpgradeMode { get; init; }
    public SpUpgradeResult UpgradeResult { get; init; }
    public short OriginalUpgrade { get; init; }
    public bool IsProtected { get; init; }
}