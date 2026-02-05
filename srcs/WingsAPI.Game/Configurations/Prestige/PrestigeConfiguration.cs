using System.Collections.Generic;
using WingsAPI.Packets.Enums.Prestige;

namespace WingsEmu.Game.Configurations.Prestige;

public class PrestigeConfiguration
{
    public List<PrestigeLevelConfig> PrestigeLevels { get; set; }
}

public class PrestigeLevelConfig
{
    public int Level { get; set; }
    public List<PrestigeTaskConfig> PrestigeTasks { get; set; }
    public PrestigeFinalChallenge FinalChallenge { get; set; }
    public PrestigeReward PlayerReachRewards { get; set; }
    public short ActiveBuffVnum { get; set; }
}

public class PrestigeRequirements
{
    public int MinPlayerLevel { get; set; }
    public int? MinPlayerHeroLevel { get; set; }
}
public class PrestigeTaskConfig
{
    public PrestigeTaskType QuestType { get; set; }
    public List<CollectItemRequirement> Items { get; set; }
    public List<KillMonsterRequirement> Monsters { get; set; }
    public int Amount { get; set; }
    public int? LevelRangeMargin { get; set; }
    public int? RaidId { get; set; }
    public int? MapVnum { get; set; }
}


public class PrestigeFinalChallenge
{
    public int BossMonsterVnum { get; set; }
    public ChallengeMap Map { get; set; }
    public PrestigeRequirements Requirements { get; set; } 
    public int TimeLimitMinutes { get; set; }
    
    public List<WarningMilestoneConfig> WarningMilestones { get; set; } = [];
}

public class WarningMilestoneConfig
{
    public int Seconds { get; set; }
    public string Key { get; set; }
}

public class ChallengeMap
{
    public int Vnum { get; set; }
    public short MonsterSpawnX { get; set; }
    public short MonsterSpawnY { get; set; }
    public short PlayerSpawnX { get; set; }
    public short PlayerSpawnY { get; set; }
}

public class PrestigeReward
{
    public int NextPrestigeLevel { get; set; }
    public short BuffVnum { get; set; }
    public short InfoBuffVnum { get; set; }
    public int? VisualEffectVnum { get; set; }
    public int RewardBoxItemVnum { get; set; }
    public int TitleVnum { get; set; }
    
    public PrestigeStatBonus AdditionalStats { get; set; } = new();
}

public class PrestigeStatBonus
{
    public int DamagePercent { get; set; }
    public int CritRatePercent { get; set; }
}

public class CollectItemRequirement
{
    public int? ItemVnum { get; set; }
    public int Amount { get; set; }
}

public class KillMonsterRequirement
{
    public int? MonsterVnum { get; set; }
    public int Amount { get; set; }
}
