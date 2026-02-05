using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Raid;
using WingsEmu.Game.Raids.Events;

namespace Plugin.PlayerLogs.Enrichers.Raid
{
    public class LogRaidSwitchButtonToggledEnricher : ILogMessageEnricher<RaidSwitchButtonToggledEvent, LogRaidSwitchButtonToggledMessage>
    {
        public void Enrich(LogRaidSwitchButtonToggledMessage message, RaidSwitchButtonToggledEvent e)
        {
            if (!e.Sender.PlayerEntity.IsInRaidParty)
            {
                return;
            }

            message.RaidId = e.Sender.PlayerEntity.Raid.Id;
            message.LeverId = e.LeverId;
        }
    }
}