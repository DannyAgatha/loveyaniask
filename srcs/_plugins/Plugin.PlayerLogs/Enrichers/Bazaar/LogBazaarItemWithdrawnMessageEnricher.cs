using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.Game.Bazaar.Events;

namespace Plugin.PlayerLogs.Enrichers.Bazaar
{
    public class LogBazaarItemWithdrawnMessageEnricher : ILogMessageEnricher<BazaarItemWithdrawnEvent, LogBazaarItemWithdrawnMessage>
    {
        public void Enrich(LogBazaarItemWithdrawnMessage message, BazaarItemWithdrawnEvent e)
        {
            message.Price = e.Price;
            message.Quantity = e.Quantity;
            message.ItemInstance = e.ItemInstance;
            message.BazaarItemId = e.BazaarItemId;
            message.ClaimedMoney = e.ClaimedMoney;
        }
    }
}