using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorLeaveEvent : PlayerEvent
{
    public bool SendMessage { get; init; }
    public bool CheckIfFinished { get; init; }
    public bool AddLeaverBuster { get; init; } = true;
}