using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public class RelictConfiguration : List<RelictConfigurationInfo>
{
}

public class RelictConfigurationInfo
{
    public int RelictVnum { get; set; }
    public int ExaminationCost { get; set; }
    public List<RelictRollCategory> Rolls { get; set; }
}

public class RelictRollCategory
{
    public int Chance { get; set; }
    public List<RelictRollItem> Items { get; set; }
}

public class RelictRollItem
{
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
}