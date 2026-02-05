using WingsEmu.DTOs.Quests;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestAddedEvent : PlayerEvent
{
    public int QuestId { get; init; }
    public QuestSlotType QuestSlotType { get; init; }
}