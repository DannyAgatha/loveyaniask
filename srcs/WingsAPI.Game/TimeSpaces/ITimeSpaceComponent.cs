using System.Collections.Generic;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.TimeSpaces;

public interface ITimeSpaceComponent
{
    public TimeSpaceParty TimeSpace { get; }
    public List<INpcEntity> Partners { get; set; }

    public bool IsInTimeSpaceParty { get; }
    public bool TimeSpaceTeamIsFull { get; }

    public void SetTimeSpaceParty(TimeSpaceParty timeSpaceParty);
    public void RemoveTimeSpaceParty();
}