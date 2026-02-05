using System;
using System.Collections.Generic;
using WingsEmu.Core;

namespace WingsEmu.Game.Act4.Configuration;

public class Act4Configuration
{
    public int MaximumFactionPoints { get; set; } = 60_000;

    public bool PveFactionPoints { get; set; } = true;
    public int FactionPointsPerPveKill { get; set; } = 60;

    public bool PvpFactionPoints { get; set; } = true;
    public int FactionPointsPerPvpKill { get; set; } = 1200;

    public List<int> BannedMapIdsToAngels { get; set; } = new() { 131 };
    public List<int> BannedMapIdsToDemons { get; set; } = new() { 130 };

    public List<PointGeneration> PointGeneration { get; set; } = new() { new PointGeneration() };

    public TimeSpan ResetDate { get; set; } = TimeSpan.Zero;

    public TimeSpan MukrajuEndSpan { get; set; } = TimeSpan.FromMinutes(5);
    public byte MukrajuRadius { get; set; }
    public MukrajuSpawn AngelMukrajuSpawn { get; set; } = new();
    public MukrajuSpawn DemonMukrajuSpawn { get; set; } = new();
}

public class MukrajuSpawn
{
    public int MonsterVnum { get; set; }
    public int MapId { get; set; }
    public short MapX { get; set; }
    public short MapY { get; set; }
}

public class PointGeneration
{
    /// <summary>
    ///     Min and max included
    /// </summary>
    /// <returns></returns>
    public Range<int?> PlayerAmount { get; set; } = new();

    public int PointsAmount { get; set; } = 100;
}