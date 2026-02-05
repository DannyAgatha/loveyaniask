using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopNpcBoughtItemMessageEnricher : ILogMessageEnricher<ShopNpcBoughtItemEvent, LogShopNpcBoughtItemMessage>
    {
        public void Enrich(LogShopNpcBoughtItemMessage message, ShopNpcBoughtItemEvent e)
        {
            message.TotalPrice = e.TotalPrice;
            message.Quantity = e.Quantity;
            message.SellerId = e.SellerId;
            message.ItemInstance = e.ItemInstance;
            message.CurrencyType = e.CurrencyType.ToString();
        }
    }
}