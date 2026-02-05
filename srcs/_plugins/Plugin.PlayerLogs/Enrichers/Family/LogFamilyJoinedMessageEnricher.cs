using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public sealed class LogFamilyJoinedMessageEnricher : ILogMessageEnricher<FamilyJoinedEvent, LogFamilyJoinedMessage>
    {
        public void Enrich(LogFamilyJoinedMessage message, FamilyJoinedEvent e)
        {
            message.FamilyId = e.FamilyId;
            message.InviterId = e.InviterId;
        }
    }
}