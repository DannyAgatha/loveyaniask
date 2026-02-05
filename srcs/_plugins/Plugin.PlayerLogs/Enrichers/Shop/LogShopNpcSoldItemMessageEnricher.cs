using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopNpcSoldItemMessageEnricher : ILogMessageEnricher<ShopNpcSoldItemEvent, LogShopNpcSoldItemMessage>
    {
        public void Enrich(LogShopNpcSoldItemMessage message, ShopNpcSoldItemEvent e)
        {
            message.Amount = e.Amount;
            message.PricePerItem = e.PricePerItem;
            message.ItemInstance = e.ItemInstance;
        }
    }
}