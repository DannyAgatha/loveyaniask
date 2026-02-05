using System;
using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.Configurations;

public interface ILandOfDeathConfig
{
    IEnumerable<LandOfDeathTimes> GetOpenTimesByChannelId(int channelId);
}

public class LandOfDeathConfig : ILandOfDeathConfig
{
    private readonly Dictionary<int, IEnumerable<LandOfDeathTimes>> _openTimes = new();

    public LandOfDeathConfig(LandOfDeathConfiguration configuration)
    {
        _openTimes = configuration.OpenTimes.ToDictionary(x => x.ChannelId, x => x.Times);
    }

    public IEnumerable<LandOfDeathTimes> GetOpenTimesByChannelId(int channelId) => _openTimes.GetValueOrDefault(channelId);
}

public class LandOfDeathConfiguration
{
    public int MinLevel { get; set; }
    public short MapSpawnPositionX { get; set; }
    public short MapSpawnPositionY { get; set; }
    public int MapVnum { get; set; }
    public List<int> SpawnItemVnums { get; set; } = [];
    public List<short> SpawnItemAmounts { get; set; } = [];
    public int DevilMonsterVnum { get; set; }
    public TimeSpan DevilStartTime { get; set; }
    public TimeSpan DevilSpawnTime { get; set; }
    public TimeSpan DevilOnMapTime { get; set; }
    public double XpMultiplier { get; set; }
    public IEnumerable<LandOfDeathOpenTimes> OpenTimes { get; set; }
    
    public int NpcAltarVnum { get; set; }
    public TimeSpan NpcAltarTimeOnMap { get; set; }
    public TimeSpan NpcAltarTimeRespawnOnMap { get; set; }
    public List<int> NpcAltarBuffVnums { get; set; } = [];
    public List<int> NpcAltarDebuffVnums { get; set; } = [];
}

public class LandOfDeathOpenTimes
{
    public int ChannelId { get; set; }
    public IEnumerable<LandOfDeathTimes> Times { get; set; }
}

public class LandOfDeathTimes
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
}