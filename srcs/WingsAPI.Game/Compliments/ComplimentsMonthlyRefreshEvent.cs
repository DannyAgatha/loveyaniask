using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Compliments;

public class ComplimentsMonthlyRefreshEvent : PlayerEvent
{
    public bool Force { get; init; }
}