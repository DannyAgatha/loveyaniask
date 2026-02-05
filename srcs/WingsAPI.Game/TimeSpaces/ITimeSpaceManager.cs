using System;
using System.Collections.Generic;

namespace WingsEmu.Game.TimeSpaces;

public interface ITimeSpaceManager
{
    IReadOnlyCollection<TimeSpaceParty> GetTimeSpaces();
    TimeSpaceParty GetTimeSpace(Guid id);
    TimeSpaceParty GetTimeSpaceByMapInstanceId(Guid id);
    TimeSpaceSubInstance GetSubInstance(Guid mapInstanceId);
    void AddTimeSpace(TimeSpaceParty timeSpaceParty);
    void AddTimeSpaceSubInstance(Guid mapInstanceId, TimeSpaceSubInstance subInstance);
    void AddTimeSpaceByMapInstanceId(Guid mapInstanceId, TimeSpaceParty timeSpaceParty);
    void RemoveTimeSpaceSubInstance(Guid mapInstanceId);
    void RemoveTimeSpacePartyByMapInstanceId(Guid mapInstanceId);
    void RemoveTimeSpace(TimeSpaceParty timeSpaceParty);
}