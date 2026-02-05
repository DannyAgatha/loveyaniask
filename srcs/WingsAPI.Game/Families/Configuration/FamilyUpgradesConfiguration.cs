using WingsAPI.Data.Families;

namespace WingsEmu.Game.Families.Configuration;

public class FamilyUpgradesConfiguration
{
    public FamilyUpgradeType UpgradeType { get; set; }
    public byte UpgradeLevel { get; set; }
    public short Value { get; set; }
}