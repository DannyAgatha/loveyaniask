using WingsEmu.Core;

namespace WingsEmu.Game.Mates.PartnerFusion;

public class PartnerFusionData
{
    public Range<int> LevelRange { get; set; }
    public int Points { get; set; }
    public int UpgradeCost { get; set; }
    public ItemRequired ItemRequired { get; set; }
    public int MajorUpgradeCost { get; set; }
    public int MajorItemAmount { get; set; }
    public int MajorPoints { get; set; }
}

public class ItemRequired
{
    public int Vnum { get; set; }
    public int Amount { get; set; }
}