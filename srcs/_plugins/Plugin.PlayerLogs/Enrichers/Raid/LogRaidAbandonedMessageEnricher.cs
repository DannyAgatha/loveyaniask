using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidAbandonedMessageEnricher : ILogMessageEnricher<RaidAbandonedEvent, LogRaidAbandonedMessage>
    {
        public void Enrich(LogRaidAbandonedMessage message, RaidAbandonedEvent e)
        {
            message.RaidId = e.RaidId;
        }
    }
}