using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Groups.Events;

public class GroupWeedingEvent : PlayerEvent
{
    public bool RemoveBuff { get; init; }
    public long? RelatedId { get; init; }
}