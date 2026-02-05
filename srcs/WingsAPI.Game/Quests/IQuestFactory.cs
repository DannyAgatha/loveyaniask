using WingsEmu.DTOs.Quests;

namespace WingsEmu.Game.Quests;

public interface IQuestFactory
{
    CharacterQuest NewQuest(long characterId, int questId, QuestSlotType questSlotType);
}