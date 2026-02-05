using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Miniland;

public class Minigame
{
    public int Vnum { get; set; }

    public MinigameType Type { get; set; }

    public int MinimumLevel { get; set; }

    public int MinimumReputation { get; set; }

    public List<MinigameRewards> Rewards { get; set; } = new()
    {
        new()
        {
            RewardLevel = RewardLevel.FirstReward,
            Rewards = new List<MinigameReward>
            {
                new()
                {
                    Amount = 1,
                    Vnum = 1
                },
                new()
                {
                    Amount = 2,
                    Vnum = 1014
                }
            }
        }
    };
}