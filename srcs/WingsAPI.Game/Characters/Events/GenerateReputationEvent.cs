using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class GenerateReputationEvent : PlayerEvent
{
    public long Amount { get; init; }
    public bool SendMessage { get; init; }
}