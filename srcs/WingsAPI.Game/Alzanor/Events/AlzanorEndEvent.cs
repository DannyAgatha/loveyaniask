using PhoenixLib.Events;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorEndEvent : IAsyncEvent
{
    public AlzanorParty AlzanorParty { get; init; }
}