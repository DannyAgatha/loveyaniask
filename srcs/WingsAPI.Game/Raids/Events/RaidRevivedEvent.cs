using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidRevivedEvent : PlayerEvent
{
    public bool RestoredLife { get; init; }
}