using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game.Configurations;

public interface ITimeSpaceConfiguration
{
    TimeSpaceFileConfiguration GetTimeSpaceConfiguration(long timeSpaceId);
    IEnumerable<TimeSpaceFileConfiguration> GetTimeSpaceConfigurationsByMapId(int mapId);
}

public class TimeSpaceConfiguration : ITimeSpaceConfiguration
{
    private readonly ImmutableDictionary<long, TimeSpaceFileConfiguration> _configurations;
    private readonly Dictionary<int, List<TimeSpaceFileConfiguration>> _configurationsByMapId = new();

    public TimeSpaceConfiguration(IEnumerable<TimeSpaceFileConfiguration> configurations)
    {
        _configurations = configurations.ToImmutableDictionary(s => s.TsId);

        foreach (TimeSpaceFileConfiguration configuration in configurations.Where(x => x.Placement != null))
        {
            foreach (TimeSpacePlacement placement in configuration.Placement)
            {
                if (!_configurationsByMapId.TryGetValue(placement.MapId, out List<TimeSpaceFileConfiguration> list))
                {
                    list = new List<TimeSpaceFileConfiguration>();
                    _configurationsByMapId[placement.MapId] = list;
                }

                list.Add(configuration);
            }
        }
    }

    public TimeSpaceFileConfiguration GetTimeSpaceConfiguration(long timeSpaceId) => _configurations.GetOrDefault(timeSpaceId);
    public IEnumerable<TimeSpaceFileConfiguration> GetTimeSpaceConfigurationsByMapId(int mapId) => _configurationsByMapId.GetOrDefault(mapId);
}

public class TimeSpaceFileConfiguration
{
    public long TsId { get; set; }
    public byte MinLevel { get; set; }
    public byte MaxLevel { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsHero { get; set; }
    public bool IsSpecial { get; set; }
    public bool IsHidden { get; set; }
    public byte MinPlayers { get; set; }
    public byte MaxPlayers { get; set; }
    public byte SeedsOfPowerRequired { get; set; }
    public TimeSpaceRewards Rewards { get; set; }
    public List<TimeSpacePlacement> Placement { get; set; }
    public int? ReputationMultiplier { get; set; }
}

public class TimeSpaceRewards
{
    public List<TimeSpaceItemConfiguration> Draw { get; set; }
    public List<TimeSpaceItemConfiguration> Special { get; set; }
    public List<TimeSpaceItemConfiguration> Bonus { get; set; }
}

public class TimeSpaceItemConfiguration
{
    public short ItemVnum { get; set; }
    public int Amount { get; set; }
}

public class TimeSpacePlacement
{
    public int MapId { get; set; }
    public short MapX { get; set; }
    public short MapY { get; set; }
}