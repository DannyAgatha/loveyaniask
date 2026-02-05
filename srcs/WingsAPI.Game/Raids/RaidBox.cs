using System.Collections.Generic;

namespace WingsEmu.Game.Raids;

public class RaidBox
{
    public RaidBox(int rewardBox, IEnumerable<RaidBoxRarity> raidBoxRarities)
    {
        RewardBox = rewardBox;
        RaidBoxRarities = raidBoxRarities;
    }

    public int RewardBox { get; }
    public IEnumerable<RaidBoxRarity> RaidBoxRarities { get; }
}