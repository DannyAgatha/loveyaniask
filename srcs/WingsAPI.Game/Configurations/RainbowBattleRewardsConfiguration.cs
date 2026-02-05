using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public class RainbowBattleReward
{
    public short ItemVnum { get; set; }
    public int Amount { get; set; }
}

public class RainbowBattleRewardsConfiguration
{
    public List<RainbowBattleReward> LowBracketRewards { get; set; } = [];
    public List<RainbowBattleReward> HighBracketRewards { get; set; } = [];
    public List<RainbowBattleReward> GeneralBracketRewards { get; set; } = [];
}