using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public interface ITimeSpaceNpcRunConfig
{
    int? GetQuestByTimeSpaceId(long timeSpaceId);
    bool DoesTimeSpaceExist(long timeSpaceId);
}

public class TimeSpaceNpcRunConfig : ITimeSpaceNpcRunConfig
{
    private readonly HashSet<long> _timeSpaces = new();
    private readonly Dictionary<long, int> _timeSpacesWithQuests = new();

    public TimeSpaceNpcRunConfig(IEnumerable<TimeSpaceNpcRunConfiguration> configurations)
    {
        foreach (TimeSpaceNpcRunConfiguration configuration in configurations)
        {
            _timeSpaces.Add(configuration.TimeSpaceId);
            if (!configuration.QuestVnum.HasValue)
            {
                continue;
            }

            _timeSpacesWithQuests[configuration.TimeSpaceId] = configuration.QuestVnum.Value;
        }
    }

    public int? GetQuestByTimeSpaceId(long timeSpaceId) => _timeSpacesWithQuests.TryGetValue(timeSpaceId, out int questVnum) ? questVnum : null;
    public bool DoesTimeSpaceExist(long timeSpaceId) => _timeSpaces.Contains(timeSpaceId);
}

public class TimeSpaceNpcRunConfiguration
{
    public int? QuestVnum { get; set; }
    public long TimeSpaceId { get; set; }
}