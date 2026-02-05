using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Quest;
using WingsEmu.Game.Quests.Event;

namespace Plugin.PlayerLogs.Enrichers.Quest
{
    public class LogQuestObjectiveUpdatedMessageEnricher : ILogMessageEnricher<QuestObjectiveUpdatedEvent, LogQuestObjectiveUpdatedMessage>
    {
        public void Enrich(LogQuestObjectiveUpdatedMessage message, QuestObjectiveUpdatedEvent e)
        {
            message.QuestId = e.CharacterQuest.QuestId;
            message.SlotType = e.CharacterQuest.SlotType.ToString();
            message.UpdatedObjectivesAmount = e.CharacterQuest.ObjectiveAmount;
        }
    }
}