using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidWonMessageEnricher : ILogMessageEnricher<RaidWonEvent, LogRaidWonMessage>
    {
        public void Enrich(LogRaidWonMessage message, RaidWonEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id.ToString();
            message.RaidType = e.Sender.PlayerEntity.Raid.Type.ToString();
        }
    }
}