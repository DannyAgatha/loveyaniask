using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.TimeSpaces;

namespace Plugin.TimeSpaces;

public class TimeSpaceManager : ITimeSpaceManager
{
    private readonly IDictionary<Guid, TimeSpaceParty> _timeSpaceParties = new Dictionary<Guid, TimeSpaceParty>();
    private readonly IDictionary<Guid, TimeSpaceParty> _timeSpacePartiesByMapInstanceId = new Dictionary<Guid, TimeSpaceParty>();
    private readonly IDictionary<Guid, TimeSpaceSubInstance> _timeSpaceSubInstances = new Dictionary<Guid, TimeSpaceSubInstance>();

    public IReadOnlyCollection<TimeSpaceParty> GetTimeSpaces() => _timeSpaceParties.Values.ToArray();

    public TimeSpaceParty GetTimeSpace(Guid id) => _timeSpaceParties.GetOrDefault(id);
    public TimeSpaceParty GetTimeSpaceByMapInstanceId(Guid id) => _timeSpacePartiesByMapInstanceId.GetOrDefault(id);

    public TimeSpaceSubInstance GetSubInstance(Guid mapInstanceId) => _timeSpaceSubInstances.GetOrDefault(mapInstanceId);

    public void AddTimeSpace(TimeSpaceParty timeSpaceParty)
    {
        _timeSpaceParties.Add(timeSpaceParty.Id, timeSpaceParty);
    }

    public void AddTimeSpaceSubInstance(Guid mapInstanceId, TimeSpaceSubInstance subInstance)
    {
        _timeSpaceSubInstances[mapInstanceId] = subInstance;
    }

    public void AddTimeSpaceByMapInstanceId(Guid mapInstanceId, TimeSpaceParty timeSpaceParty)
    {
        _timeSpacePartiesByMapInstanceId[mapInstanceId] = timeSpaceParty;
    }

    public void RemoveTimeSpaceSubInstance(Guid mapInstanceId)
    {
        _timeSpaceSubInstances.Remove(mapInstanceId);
    }

    public void RemoveTimeSpacePartyByMapInstanceId(Guid mapInstanceId)
    {
        _timeSpacePartiesByMapInstanceId.Remove(mapInstanceId);
    }

    public void RemoveTimeSpace(TimeSpaceParty timeSpaceParty) => _timeSpaceParties.Remove(timeSpaceParty.Id);
}