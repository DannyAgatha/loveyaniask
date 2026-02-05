using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidInvitedMessageEnricher : ILogMessageEnricher<RaidInvitedEvent, LogRaidInvitedMessage>
    {
        public void Enrich(LogRaidInvitedMessage message, RaidInvitedEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
            message.TargetId = e.TargetId;
        }
    }
}