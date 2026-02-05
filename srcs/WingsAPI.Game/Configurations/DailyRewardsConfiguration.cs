using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public class DailyRewardsConfiguration
{
    public DailyRewards Rewards { get; set; }
}

public class DailyRewards
{
    public List<DailyRewardItem> Items { get; set; }

    public List<DailyRewardReputation> Reputations { get; set; }

    public List<DailyRewardGold> Golds { get; set; }
}

public class DailyRewardItem
{
    public string Time { get; set; }

    public int ItemVnum { get; set; }

    public int Amount { get; set; }
}

public class DailyRewardReputation
{
    public string Time { get; set; }

    public int Amount { get; set; }
}

public class DailyRewardGold
{
    public string Time { get; set; }
    public int Amount { get; set; }
}