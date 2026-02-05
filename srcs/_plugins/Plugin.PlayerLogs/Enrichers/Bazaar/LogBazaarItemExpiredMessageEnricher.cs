using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.Game.Bazaar.Events;

namespace Plugin.PlayerLogs.Enrichers.Bazaar
{
    public class LogBazaarItemExpiredMessageEnricher : ILogMessageEnricher<BazaarItemExpiredEvent, LogBazaarItemExpiredMessage>
    {
        public void Enrich(LogBazaarItemExpiredMessage message, BazaarItemExpiredEvent e)
        {
            message.Item = e.Item;
            message.Price = e.Price;
            message.Quantity = e.Quantity;
            message.BazaarItemId = e.BazaarItemId;
        }
    }
}