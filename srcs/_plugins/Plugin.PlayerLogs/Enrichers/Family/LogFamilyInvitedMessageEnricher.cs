using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyInvitedMessageEnricher : ILogMessageEnricher<FamilyInvitedEvent, LogFamilyInvitedMessage>
    {
        public void Enrich(LogFamilyInvitedMessage message, FamilyInvitedEvent e)
        {
            message.FamilyId = e.Sender.PlayerEntity.Family.Id;
            message.TargetId = e.TargetId;
        }
    }
}