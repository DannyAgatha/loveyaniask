using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidRewardReceivedEvent : PlayerEvent
{
    public byte BoxRarity { get; init; }
}