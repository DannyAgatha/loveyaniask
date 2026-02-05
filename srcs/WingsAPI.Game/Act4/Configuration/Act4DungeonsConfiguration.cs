using System;
using System.Collections.Generic;

namespace WingsEmu.Game.Act4.Configuration;

public class Act4DungeonsConfiguration
{
    public int DungeonPortalMapId { get; set; }
    public short DungeonPortalMapX { get; set; }
    public short DungeonPortalMapY { get; set; }

    public int DungeonReturnPortalMapId { get; set; }
    public short DungeonReturnPortalMapX { get; set; }
    public short DungeonReturnPortalMapY { get; set; }

    public int DungeonEntryCostMultiplier { get; set; }
    public TimeSpan DungeonDeathRevivalDelay { get; set; } = TimeSpan.FromSeconds(20);
    public TimeSpan DungeonDuration { get; set; } = TimeSpan.FromMinutes(60);
    public TimeSpan DungeonBossMapClosureAfterReward { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DungeonSlowMoDelay { get; set; } = TimeSpan.FromSeconds(7);

    public List<GuardianSpawn> GuardiansForAngels { get; set; } = new() { new GuardianSpawn() };
    public List<GuardianSpawn> GuardiansForDemons { get; set; } = new() { new GuardianSpawn() };
}

public class GuardianSpawn
{
    public int MonsterVnum { get; set; }
    public short MapX { get; set; }
    public short MapY { get; set; }
    public byte Direction { get; set; }
}