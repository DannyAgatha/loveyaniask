using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Quest;
using WingsEmu.Game.Quests.Event;

namespace Plugin.PlayerLogs.Enrichers.Quest
{
    public class LogQuestCompletedMessageEnricher : ILogMessageEnricher<QuestCompletedLogEvent, LogQuestCompletedMessage>
    {
        public void Enrich(LogQuestCompletedMessage message, QuestCompletedLogEvent e)
        {
            message.QuestId = e.CharacterQuest.QuestId;
            message.SlotType = e.CharacterQuest.SlotType.ToString();
            message.Location = e.Location;
        }
    }
}