using System;
using System.Collections.Generic;
using WingsEmu.Game.Configurations.Miniland;

namespace WingsEmu.Game.Miniland;

public class MinilandInteractionInformationHolder
{
    public MinilandInteractionInformationHolder(MinigameInteraction interaction, MapDesignObject mapObject)
    {
        Interaction = interaction;
        TimeOfInteraction = DateTime.UtcNow;
        MapObject = mapObject;
    }

    public MinilandInteractionInformationHolder(MinigameInteraction interaction, MapDesignObject mapObject, (RewardLevel maxRewardLevel, List<MinigameRewards> rewards) rewards)
    {
        Interaction = interaction;
        TimeOfInteraction = DateTime.UtcNow;
        MapObject = mapObject;
        SavedRewards = rewards;
    }

    public MinigameInteraction Interaction { get; set; }

    public DateTime TimeOfInteraction { get; set; }

    public MapDesignObject MapObject { get; set; }

    public (RewardLevel maxRewardLevel, List<MinigameRewards> rewards) SavedRewards { get; set; }
}