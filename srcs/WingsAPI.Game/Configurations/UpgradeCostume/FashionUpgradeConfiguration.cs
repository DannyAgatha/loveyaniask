using System.Collections.Generic;
using WingsAPI.Packets.Enums.CostumeUpgrade;

namespace WingsEmu.Game.Configurations.UpgradeCostume;

public class FashionUpgradeConfiguration
{
    public List<FashionUpgradeType> FashionUpgrade { get; set; }
}

public class FashionUpgradeType
{
    public UpgradeCostumeType UpgradeType { get; set; }
    public List<FashionUpgradeSet> Sets { get; set; }
}

public class FashionUpgradeSet
{
    public int RequiredFashionVnum { get; set; }
    public int ObtainedFashionVnum { get; set; }
    public double Chance { get; set; }
    public List<Material> Materials { get; set; }
}

public class Material
{
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
}