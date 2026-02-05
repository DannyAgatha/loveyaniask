using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class QuestRemoveEvent : PlayerEvent
{
    public QuestRemoveEvent(CharacterQuest characterQuest, bool isCompleted)
    {
        CharacterQuest = characterQuest;
        IsCompleted = isCompleted;
    }

    public CharacterQuest CharacterQuest { get; }
    public bool IsCompleted { get; }
}