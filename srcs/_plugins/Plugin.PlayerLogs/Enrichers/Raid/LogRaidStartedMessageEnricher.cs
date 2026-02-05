using System.Linq;
using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidStartedMessageEnricher : ILogMessageEnricher<RaidStartedEvent, LogRaidStartedMessage>
    {
        public void Enrich(LogRaidStartedMessage message, RaidStartedEvent e)
        {
            RaidParty raidParty = e.Sender.PlayerEntity.Raid;
            message.MembersIds = raidParty.Members.Select(x => x.Account.Id).ToArray();
            message.RaidId = raidParty.Id;
            message.RaidType = raidParty.Type.ToString();
        }
    }
}