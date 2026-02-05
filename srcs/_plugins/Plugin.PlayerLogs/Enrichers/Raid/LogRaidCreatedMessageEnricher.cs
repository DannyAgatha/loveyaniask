using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidCreatedMessageEnricher : ILogMessageEnricher<RaidCreatedEvent, LogRaidCreatedMessage>
    {
        public void Enrich(LogRaidCreatedMessage message, RaidCreatedEvent e)
        {
            RaidParty raidParty = e.Sender.PlayerEntity.Raid;
            message.RaidId = raidParty.Id;
            message.RaidType = raidParty.Type.ToString();
        }
    }
}