using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.Game.Bazaar.Events;

namespace Plugin.PlayerLogs.Enrichers.Bazaar
{
    public class LogBazaarItemInsertedMessageEnricher : ILogMessageEnricher<BazaarItemInsertedEvent, LogBazaarItemInsertedMessage>
    {
        public void Enrich(LogBazaarItemInsertedMessage message, BazaarItemInsertedEvent e)
        {
            message.Price = e.Price;
            message.Quantity = e.Quantity;
            message.ItemInstance = e.ItemInstance;
            message.BazaarItemId = e.BazaarItemId;
            message.Taxes = e.Taxes;
        }
    }
}