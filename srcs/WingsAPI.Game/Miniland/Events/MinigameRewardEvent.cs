using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Configurations.Miniland;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameRewardEvent : PlayerEvent
{
    public MinigameRewardEvent(RewardLevel rewardLevel, MapDesignObject mapObject, bool coupon)
    {
        RewardLevel = rewardLevel;
        MapObject = mapObject;
        Coupon = coupon;
    }

    public RewardLevel RewardLevel { get; }

    public MapDesignObject MapObject { get; }

    public bool Coupon { get; }
}