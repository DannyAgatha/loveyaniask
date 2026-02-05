using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Act7.Tattoos;

public class TattooUpgradeItem
{
    public int Vnum { get; set; }
    public short Quantity { get; set; }
}

public class TattooUpgrade
{
    public int Upgrade { get; set; }
    public int Success { get; set; }
    public int MajorFail { get; set; }
    public int Gold { get; set; }
    public List<TattooUpgradeItem> Items { get; set; }
}

public class TattooUpgradeConfiguration
{
    public List<TattooUpgrade> TattooUpgrade { get; set; }
}
