using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestObjectiveUpdatedEvent : PlayerEvent
{
    public CharacterQuest CharacterQuest { get; init; }
}