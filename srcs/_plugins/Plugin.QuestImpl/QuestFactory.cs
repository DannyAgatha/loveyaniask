using WingsEmu.DTOs.Quests;
using WingsEmu.Game;
using WingsEmu.Game.Quests;

namespace Plugin.QuestImpl
{
    public class QuestFactory : IQuestFactory
    {
        private readonly IQuestManager _questManager;
        private readonly IRandomGenerator _randomGenerator;

        public QuestFactory(IQuestManager questManager, IRandomGenerator randomGenerator)
        {
            _questManager = questManager;
            _randomGenerator = randomGenerator;
        }

        public CharacterQuest NewQuest(long characterId, int questId, QuestSlotType questSlotType)
        {
            QuestDto quest = _questManager.GetQuestById(questId);
            return quest == null ? null : new CharacterQuest(quest, questSlotType, _randomGenerator);
        }
    }
}