using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidDiedMessageEnricher : ILogMessageEnricher<RaidDiedEvent, LogRaidDiedMessage>

    {
        public void Enrich(LogRaidDiedMessage message, RaidDiedEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
        }
    }
}