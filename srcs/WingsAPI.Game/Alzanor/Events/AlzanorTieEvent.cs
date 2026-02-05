using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorTieEvent : PlayerEvent
{
    public int[] RedTeam { get; init; }
    public int[] BlueTeam { get; init; }
}