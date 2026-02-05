using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.UpgradeCostume;

public class UpgradeCostumeConfiguration
{
    public int GoldCost { get; set; }
    public List<ItemReward> UpgradeCostumeRewards { get; set; }
}

public class ItemReward
{
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
    public int Probability { get; set; }
}