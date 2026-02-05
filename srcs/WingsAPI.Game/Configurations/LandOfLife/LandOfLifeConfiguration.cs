using System;
using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.Configurations.LandOfLife;

public interface ILandOfLifeConfig
{
    IEnumerable<LandOfLifeTimes> GetOpenTimesByChannelId(int channelId);
}

public class LandOfLifeConfig : ILandOfLifeConfig
{
    private readonly Dictionary<int, IEnumerable<LandOfLifeTimes>> _openTimes;

    public LandOfLifeConfig(LandOfLifeConfiguration configuration)
    {
        _openTimes = configuration.OpenTimes.ToDictionary(x => x.ChannelId, x => x.Times);
    }

    public IEnumerable<LandOfLifeTimes> GetOpenTimesByChannelId(int channelId) => _openTimes.GetValueOrDefault(channelId);
}

public class LandOfLifeConfiguration
{
    public List<LandOfLifeTypeConfig> LandOfLifeTypes { get; set; } = [];
    public List<int> SpawnItemVnums { get; set; } = [];
    public List<short> SpawnItemAmounts { get; set; } = [];
    public TimeSpan DragonStartTime { get; set; }
    public TimeSpan DragonSpawnTime { get; set; }
    public TimeSpan DragonOnMapTime { get; set; }
    
    public double XpMultiplier { get; set; }
    public IEnumerable<LandOfLifeOpenTimes> OpenTimes { get; set; }
    public List<Dictionary<string, TimeSpan>> MaxDailyPlaytimePerDay { get; set; } = [];
    public int NpcAltarVnum { get; set; }
    public TimeSpan NpcAltarTimeOnMap { get; set; }
    public TimeSpan NpcAltarTimeRespawnOnMap { get; set; }
    public List<int> NpcAltarBuffVnums { get; set; } = [];
    public List<int> NpcAltarDebuffVnums { get; set; } = [];
    
    public TimeSpan GetMaxPlaytimeForToday()
    {
        string today = DateTime.UtcNow.DayOfWeek.ToString().ToLowerInvariant();
        return MaxDailyPlaytimePerDay
            .FirstOrDefault(dict => dict.TryGetValue(today, out _))?
            .GetValueOrDefault(today) ?? TimeSpan.Zero;
    }
}

public class LandOfLifeOpenTimes
{
    public int ChannelId { get; set; }
    public IEnumerable<LandOfLifeTimes> Times { get; set; }
}

public class LandOfLifeTimes
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
}

public class LandOfLifeTypeConfig
{
    public string RunType { get; set; }
    public int? MinHeroLevelRequired { get; set; }
    public int? MaxHeroLevelRequired { get; set; }
    public short MapSpawnPositionX { get; set; }
    public short MapSpawnPositionY { get; set; }
    public int MapVnum { get; set; }
    public int DragonMonsterVnum { get; set; }
}
