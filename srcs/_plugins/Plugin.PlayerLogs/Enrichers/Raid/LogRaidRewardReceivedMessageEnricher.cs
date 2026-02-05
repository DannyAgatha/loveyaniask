using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidRewardReceivedMessageEnricher : ILogMessageEnricher<RaidRewardReceivedEvent, LogRaidRewardReceivedMessage>
    {
        public void Enrich(LogRaidRewardReceivedMessage message, RaidRewardReceivedEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
            message.BoxRarity = e.BoxRarity;
        }
    }
}