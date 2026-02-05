using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyKickedMessageEnricher : ILogMessageEnricher<FamilyKickedMemberEvent, LogFamilyKickedMessage>
    {
        public void Enrich(LogFamilyKickedMessage message, FamilyKickedMemberEvent e)
        {
            message.FamilyId = e.Sender.PlayerEntity.Family.Id;
            message.KickedMemberId = e.KickedMemberId;
        }
    }
}