using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidLostMessageEnricher : ILogMessageEnricher<RaidLostEvent, LogRaidLostMessage>
    {
        public void Enrich(LogRaidLostMessage message, RaidLostEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id.ToString();
            message.RaidType = e.Sender.PlayerEntity.Raid.Type.ToString();
        }
    }
}