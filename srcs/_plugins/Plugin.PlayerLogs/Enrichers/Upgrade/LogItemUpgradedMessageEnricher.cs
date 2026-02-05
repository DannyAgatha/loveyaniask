using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogItemUpgradedMessageEnricher : ILogMessageEnricher<ItemUpgradedEvent, LogItemUpgradedMessage>
    {
        public void Enrich(LogItemUpgradedMessage message, ItemUpgradedEvent e)
        {
            message.Item = e.Item;
            message.Mode = e.Mode.ToString();
            message.Protection = e.Protection.ToString();
            message.HasAmulet = e.HasAmulet;
            message.OriginalUpgrade = e.OriginalUpgrade;
            message.Result = e.Result.ToString();
            message.TotalPrice = e.TotalPrice;
        }
    }
}