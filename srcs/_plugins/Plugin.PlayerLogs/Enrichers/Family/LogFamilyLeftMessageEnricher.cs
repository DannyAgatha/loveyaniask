using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public sealed class LogFamilyLeftMessageEnricher : ILogMessageEnricher<FamilyLeftEvent, LogFamilyLeftMessage>
    {
        public void Enrich(LogFamilyLeftMessage message, FamilyLeftEvent e)
        {
            message.FamilyId = e.FamilyId;
        }
    }
}