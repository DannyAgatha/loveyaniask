using WingsEmu.Game._packetHandling;
using WingsEmu.Game.TimeSpaces.Enums;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceInstanceFinishEvent : PlayerEvent
{
    public TimeSpaceInstanceFinishEvent(TimeSpaceParty timeSpaceParty, TimeSpaceFinishType timeSpaceFinishType, long? victimId = null)
    {
        TimeSpaceParty = timeSpaceParty;
        TimeSpaceFinishType = timeSpaceFinishType;
        VictimId = victimId;
    }

    public TimeSpaceParty TimeSpaceParty { get; }
    public TimeSpaceFinishType TimeSpaceFinishType { get; }
    public long? VictimId { get; }
}