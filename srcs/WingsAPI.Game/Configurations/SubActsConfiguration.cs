using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game.Configurations;

public interface ISubActConfiguration
{
    IEnumerable<SubActsConfiguration> GetConfigurations();
    SubActsConfiguration GetConfigurationById(long id);
    SubActsConfiguration GetConfigurationByTimeSpaceId(long id);
}

public class SubActConfiguration : ISubActConfiguration
{
    private readonly List<SubActsConfiguration> _configurations;
    private readonly Dictionary<long, SubActsConfiguration> _configurationsById = new();
    private readonly Dictionary<long, SubActsConfiguration> _configurationsByTimeSpaceId = new();

    public SubActConfiguration(IEnumerable<SubActsConfiguration> configurations)
    {
        _configurations = configurations.ToList();

        foreach (SubActsConfiguration configuration in _configurations)
        {
            _configurationsById[configuration.Id] = configuration;
        }

        foreach (SubActsConfiguration configuration in configurations.Where(x => x.TimeSpaces != null))
        {
            foreach (long timeSpaceId in configuration.TimeSpaces)
            {
                _configurationsByTimeSpaceId[timeSpaceId] = configuration;
            }
        }
    }

    public IEnumerable<SubActsConfiguration> GetConfigurations() => _configurations;

    public SubActsConfiguration GetConfigurationById(long id) => _configurationsById.GetOrDefault(id);

    public SubActsConfiguration GetConfigurationByTimeSpaceId(long id) => _configurationsByTimeSpaceId.GetOrDefault(id);
}

public class SubActsConfiguration
{
    public short Id { get; set; }
    public byte Act { get; set; }
    public byte SubAct { get; set; }
    public long[] TimeSpaces { get; set; }
}