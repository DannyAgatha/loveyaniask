using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidJoinedMessageEnricher : ILogMessageEnricher<RaidJoinedEvent, LogRaidJoinedMessage>
    {
        public void Enrich(LogRaidJoinedMessage message, RaidJoinedEvent e)
        {
            message.RaidJoinType = e.JoinType.ToString();
            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
        }
    }
}