using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyMessageMessageEnricher : ILogMessageEnricher<FamilyMessageSentEvent, LogFamilyMessageMessage>
    {
        public void Enrich(LogFamilyMessageMessage message, FamilyMessageSentEvent e)
        {
            message.FamilyId = e.Sender.PlayerEntity.Family.Id;
            message.Message = e.Message;
            message.FamilyMessageType = e.MessageType.ToString();
        }
    }
}