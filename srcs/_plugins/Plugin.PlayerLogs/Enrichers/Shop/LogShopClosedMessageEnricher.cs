using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopClosedMessageEnricher : ILogMessageEnricher<ShopClosedEvent, LogShopClosedMessage>
    {
        public void Enrich(LogShopClosedMessage message, ShopClosedEvent e)
        {
            message.Location = e.Sender.GetLocation();
        }
    }
}