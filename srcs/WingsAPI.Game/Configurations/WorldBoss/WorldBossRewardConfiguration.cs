using System.Collections.Generic;
using WingsAPI.Packets.Enums.WorldBoss;

namespace WingsEmu.Game.Configurations.WorldBoss;

public class WorldBossRewardConfiguration
{
    public Dictionary<WorldBossModeType, List<WorldBossBossRewards>> WorldBossRewards { get; set; } = new();
}

public class WorldBossBossRewards
{
    public int BossVnum { get; set; }
    public List<WorldBossRewardEntry> GuaranteedRewards { get; set; } = [];
    public List<WorldBossRewardEntry> LotteryRewards { get; set; } = [];
}

public class WorldBossRewardEntry
{
    public WorldBossRewardType Type { get; set; }
    public int Quantity { get; set; }
    public int ItemVnum { get; set; }
}