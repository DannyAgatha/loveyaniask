using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopPlayerBoughtItemMessageEnricher : ILogMessageEnricher<ShopPlayerBoughtItemEvent, LogShopPlayerBoughtItemMessage>
    {
        public void Enrich(LogShopPlayerBoughtItemMessage message, ShopPlayerBoughtItemEvent e)
        {
            message.TotalPrice = e.TotalPrice;
            message.Quantity = e.Quantity;
            message.SellerId = e.SellerId;
            message.SellerName = e.SellerName;
            message.ItemInstance = e.ItemInstance;
        }
    }
}