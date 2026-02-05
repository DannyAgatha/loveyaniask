using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopSkillSoldMessageEnricher : ILogMessageEnricher<ShopSkillSoldEvent, LogShopSkillSoldMessage>
    {
        public void Enrich(LogShopSkillSoldMessage message, ShopSkillSoldEvent e)
        {
            message.SkillVnum = e.SkillVnum;
        }
    }
}