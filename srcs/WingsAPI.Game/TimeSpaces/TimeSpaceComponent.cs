using System.Collections.Generic;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceComponent : ITimeSpaceComponent
{
    public TimeSpaceParty TimeSpace { get; private set; }
    public List<INpcEntity> Partners { get; set; } = new();
    public bool TimeSpaceTeamIsFull => TimeSpace != null && TimeSpace.Members.Count >= TimeSpace.TimeSpaceInformation.MaxPlayers;

    public bool IsInTimeSpaceParty => TimeSpace != null;

    public void SetTimeSpaceParty(TimeSpaceParty timeSpaceParty)
    {
        TimeSpace = timeSpaceParty;
    }

    public void RemoveTimeSpaceParty()
    {
        TimeSpace = null;
    }
}