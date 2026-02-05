using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopOpenedMessageEnricher : ILogMessageEnricher<ShopOpenedEvent, LogShopOpenedMessage>
    {
        public void Enrich(LogShopOpenedMessage message, ShopOpenedEvent e)
        {
            message.Location = e.Sender.GetLocation();
            message.ShopName = e.ShopName;
        }
    }
}