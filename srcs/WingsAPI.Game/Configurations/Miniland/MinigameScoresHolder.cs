using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Miniland;

public class MinigameScoresHolder
{
    public MinigameType Type { get; set; }

    public List<ScoreHolder> Scores { get; set; } = new()
    {
        new()
    };
}