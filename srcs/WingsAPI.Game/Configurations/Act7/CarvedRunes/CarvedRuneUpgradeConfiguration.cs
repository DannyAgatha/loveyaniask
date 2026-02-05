using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Act7.CarvedRunes;

public class CarvedRuneUpgradeConfiguration
{
    public short MaxUpgrade { get; set; }
    public List<CarvedRuneUpgrade> RuneUpgradeItem { get; set; }
}

public class CarvedRuneUpgrade
{
    public int Upgrade { get; set; }
    public double Success { get; set; }
    public double Damage { get; set; }
    public int Gold { get; set; }
    public List<CarvedRuneUpgradeItem> ItemsWeapon { get; set; }
    public List<CarvedRuneUpgradeItem> ItemsArmor { get; set; }
}

public class CarvedRuneUpgradeItem
{
    public int Vnum { get; set; }
    public short Quantity { get; set; }
}