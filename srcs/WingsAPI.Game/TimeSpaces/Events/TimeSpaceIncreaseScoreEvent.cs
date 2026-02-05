using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceIncreaseScoreEvent : PlayerEvent
{
    public int AmountToIncrease { get; init; }
    public TimeSpaceParty TimeSpaceParty { get; init; }
}