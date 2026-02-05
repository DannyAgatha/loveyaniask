using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Quest;
using WingsEmu.Game.Quests.Event;

namespace Plugin.PlayerLogs.Enrichers.Quest
{
    public class LogQuestAbandonedMessageEnricher : ILogMessageEnricher<QuestAbandonedEvent, LogQuestAbandonedMessage>
    {
        public void Enrich(LogQuestAbandonedMessage message, QuestAbandonedEvent e)
        {
            message.QuestId = e.QuestId;
            message.SlotType = e.QuestSlotType.ToString();
        }
    }
}