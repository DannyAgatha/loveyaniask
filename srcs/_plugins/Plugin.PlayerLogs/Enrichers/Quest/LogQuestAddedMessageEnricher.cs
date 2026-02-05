using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Quest;
using WingsEmu.Game.Quests.Event;

namespace Plugin.PlayerLogs.Enrichers.Quest
{
    public class LogQuestAddedMessageEnricher : ILogMessageEnricher<QuestAddedEvent, LogQuestAddedMessage>
    {
        public void Enrich(LogQuestAddedMessage message, QuestAddedEvent e)
        {
            message.QuestId = e.QuestId;
            message.SlotType = e.QuestSlotType.ToString();
        }
    }
}