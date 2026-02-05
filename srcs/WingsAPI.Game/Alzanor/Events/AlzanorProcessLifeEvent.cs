using PhoenixLib.Events;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorProcessLifeEvent : IAsyncEvent
{
    public AlzanorParty AlzanorParty { get; init; }
}