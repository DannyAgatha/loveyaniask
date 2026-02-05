using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.Game.Shops.Event;

namespace Plugin.PlayerLogs.Enrichers.Shop
{
    public class LogShopSkillBoughtMessageEnricher : ILogMessageEnricher<ShopSkillBoughtEvent, LogShopSkillBoughtMessage>
    {
        public void Enrich(LogShopSkillBoughtMessage boughtMessage, ShopSkillBoughtEvent e)
        {
            boughtMessage.SkillVnum = e.SkillVnum;
        }
    }
}