using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidTargetKilledMessageEnricher : ILogMessageEnricher<RaidTargetKilledEvent, LogRaidTargetKilledMessage>
    {
        public void Enrich(LogRaidTargetKilledMessage message, RaidTargetKilledEvent e)
        {
            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
            message.DamagerCharactersIds = e.DamagerCharactersIds;
        }
    }
}