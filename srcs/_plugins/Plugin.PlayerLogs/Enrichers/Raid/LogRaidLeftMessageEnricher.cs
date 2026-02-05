using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidLeftMessageEnricher : ILogMessageEnricher<RaidLeftEvent, LogRaidLeftMessage>
    {
        public void Enrich(LogRaidLeftMessage message, RaidLeftEvent e)
        {
            message.RaidId = e.RaidId;
        }
    }
}