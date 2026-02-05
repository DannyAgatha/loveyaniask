using System.Collections.Generic;
using WingsAPI.Packets.Enums;

namespace WingsEmu.Game.Configurations.RaidExtraRewards;

public class Reward
{
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
    public byte Upgrade { get; set; }
    public sbyte Rarity { get; set; }
    public int Chance { get; set; }
}

public class ExtraReward
{
    public int ItemVnum { get; set; }
    
    public int Amount { get; set; }
    
    public int Chance { get; set; }
}

public class RaidExtraReward
{
    public RaidType RaidType { get; set; }
    
    public Reward RewardLegendary { get; set; }
    
    public Reward RewardPhenomenal { get; set; }
    
    public List<ExtraReward> ExtraRewards { get; set; } = [];
}

public class RaidExtraRewardsConfiguration
{
    public List<RaidExtraReward> RaidExtraRewards { get; set; } = [];
}


