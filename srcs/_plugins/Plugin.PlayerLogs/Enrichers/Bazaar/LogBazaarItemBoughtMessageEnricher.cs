using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.Game.Bazaar.Events;

namespace Plugin.PlayerLogs.Enrichers.Bazaar
{
    public class LogBazaarItemBoughtMessageEnricher : ILogMessageEnricher<BazaarItemBoughtEvent, LogBazaarItemBoughtMessage>
    {
        public void Enrich(LogBazaarItemBoughtMessage message, BazaarItemBoughtEvent e)
        {
            message.BazaarItemId = e.BazaarItemId;
            message.SellerId = e.SellerId;
            message.SellerName = e.SellerName;
            message.PricePerItem = e.PricePerItem;
            message.BoughtItem = e.BoughtItem;
            message.Amount = e.Amount;
        }
    }
}