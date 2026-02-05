using PhoenixLib.Events;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorDestroyEvent : IAsyncEvent
{
    public AlzanorParty AlzanorParty { get; init; }
}