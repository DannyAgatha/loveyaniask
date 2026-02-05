using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidRevivedMessageEnricher : ILogMessageEnricher<RaidRevivedEvent, LogRaidRevivedMessage>
    {
        public void Enrich(LogRaidRevivedMessage message, RaidRevivedEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
            message.RestoredLife = e.RestoredLife;
        }
    }
}