using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyDisbandedMessageEnricher : ILogMessageEnricher<FamilyDisbandedEvent, LogFamilyDisbandedMessage>
    {
        public void Enrich(LogFamilyDisbandedMessage message, FamilyDisbandedEvent e)
        {
            message.FamilyId = e.FamilyId;
        }
    }
}