using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations.Miniland;

public class GlobalMinigameConfiguration
{
    public int MaxmimumMinigamePoints { get; set; } = 2_000;

    public short MinigamePointsCostPerMinigame { get; set; } = 100;

    public int ProductionCouponVnum { get; set; } = (int)ItemVnums.PRODUCTION_COUPON;

    public short ProductionCouponPointsAmount { get; set; } = 500;

    public long RepairDurabilityGoldCost { get; set; } = 100;

    public int RepairDurabilityCouponVnum { get; set; } = (int)ItemVnums.DURABILITY_COUPON;

    public int DurabilityCouponRepairingAmount { get; set; } = 300;

    public int DurabilityWarning { get; set; } = 1_000;

    public int MinigameMaximumRewards { get; set; } = 999;

    public int DoubleRewardCouponVnum { get; set; } = (int)ItemVnums.REWARD_COUPON;

    public int MinigameRewardsInventoryWarning { get; set; } = 880;

    public int MinigameRewardsInventoryUltimatum { get; set; } = 970;

    public AntiExploitConfiguration AntiExploitConfiguration { get; set; } = new();
}