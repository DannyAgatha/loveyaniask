using WingsAPI.Data.Families;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyUpgradeBoughtEvent : PlayerEvent
{
    public long FamilyId { get; init; }
    public int UpgradeVnum { get; init; }
    public FamilyUpgradeType FamilyUpgradeType { get; init; }
    public short UpgradeValue { get; init; }
}