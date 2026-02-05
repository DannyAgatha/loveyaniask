using System;
using System.Collections.Generic;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Prestige;

public class PrestigeInstance
{
    public PrestigeInstance(
        IMapInstance mapInstance,
        int timeLimitMinutes,
        List<WarningMilestoneConfig> warningMilestones)
    {
        MapInstance = mapInstance;
        CreationTime = DateTime.UtcNow;
        TimeLimit = TimeSpan.FromMinutes(timeLimitMinutes);
        WarnedMilestones = new HashSet<int>();
        WarningMilestones = warningMilestones ?? new List<WarningMilestoneConfig>();
    }

    public IMapInstance MapInstance { get; }
    public DateTime CreationTime { get; }
    public TimeSpan TimeLimit { get; }
    public HashSet<int> WarnedMilestones { get; }
    public List<WarningMilestoneConfig> WarningMilestones { get; }
    public IMonsterEntity? BossEntity { get; set; }
}

