using System;
using WingsEmu.Core;

namespace WingsEmu.Game.Configurations.Miniland;

public class ScoreHolder
{
    public RewardLevel RewardLevel { get; set; } = RewardLevel.NoReward;

    public Range<int> ScoreRange { get; set; }

    public TimeSpan MinimumTimeOfCompletion { get; set; }
}