using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Configurations.Miniland;

namespace WingsEmu.Game.Miniland.Events;

public class MinigameRewardClaimedEvent : PlayerEvent
{
    public long OwnerId { get; init; }
    public int MinigameVnum { get; init; }
    public MinigameType MinigameType { get; init; }
    public RewardLevel RewardLevel { get; init; }
    public bool Coupon { get; init; }
    public int ItemVnum { get; init; }
    public int Amount { get; init; }
}