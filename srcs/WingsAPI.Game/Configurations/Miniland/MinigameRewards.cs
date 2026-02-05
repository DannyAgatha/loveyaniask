using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Miniland;

public class MinigameRewards
{
    public RewardLevel RewardLevel { get; set; }

    public ushort DurabilityCost { get; set; }

    public List<MinigameReward> Rewards { get; set; } = new();
}