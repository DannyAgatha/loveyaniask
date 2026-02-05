using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act7.CarvedRunes;

public class ArmorCarvedRuneUpgradedEvent : PlayerEvent
{
    public ItemInstanceDTO Armor { get; init; }
    public CarvedRuneUpgradeResult UpgradeResult { get; init; }
    public short OriginalUpgrade { get; init; }
    public bool IsProtected { get; init; }
}
