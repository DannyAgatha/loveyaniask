using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidInstanceLivesIncDecEvent : PlayerEvent
{
    public RaidInstanceLivesIncDecEvent(short amount) => Amount = amount;

    public short Amount { get; }
}