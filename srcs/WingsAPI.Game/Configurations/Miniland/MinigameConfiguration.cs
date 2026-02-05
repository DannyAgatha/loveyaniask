using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Miniland;

public class MinigameConfiguration
{
    public List<Minigame> Minigames { get; set; } = new()
    {
        new()
    };

    public List<MinigameScoresHolder> ScoresHolders { get; set; } = new()
    {
        new()
    };

    public GlobalMinigameConfiguration Configuration { get; set; } = new();
}