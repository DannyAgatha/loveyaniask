using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Act7.CarvedRunes;

public class WeaponCarvedRuneUpgradedEvent : PlayerEvent
{
    public ItemInstanceDTO Weapon { get; init; }
    public CarvedRuneUpgradeResult UpgradeResult { get; init; }
    public short OriginalUpgrade { get; init; }
    public bool IsProtected { get; init; }
}
