using WingsEmu.DTOs.Quests;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class AddQuestEvent : PlayerEvent
{
    public AddQuestEvent(int questId, QuestSlotType questSlotType)
    {
        QuestId = questId;
        QuestSlotType = questSlotType;
    }

    public int QuestId { get; }
    public QuestSlotType QuestSlotType { get; }
}