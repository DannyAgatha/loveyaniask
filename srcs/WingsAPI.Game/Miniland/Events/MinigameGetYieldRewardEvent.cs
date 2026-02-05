using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Configurations.Miniland;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameGetYieldRewardEvent : PlayerEvent
{
    public MinigameGetYieldRewardEvent(MapDesignObject mapObject, RewardLevel rewardLevel)
    {
        MapObject = mapObject;
        RewardLevel = rewardLevel;
    }

    public MapDesignObject MapObject { get; }

    public RewardLevel RewardLevel { get; }
}