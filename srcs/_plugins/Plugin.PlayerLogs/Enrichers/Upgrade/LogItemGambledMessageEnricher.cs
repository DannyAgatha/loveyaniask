using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogItemGambledMessageEnricher : ILogMessageEnricher<ItemGambledEvent, LogItemGambledMessage>
    {
        public void Enrich(LogItemGambledMessage message, ItemGambledEvent e)
        {
            message.ItemVnum = e.ItemVnum;
            message.Mode = e.Mode.ToString();
            message.Protection = e.Protection.ToString();
            message.Amulet = e.Amulet;
            message.Succeed = e.Succeed;
            message.OriginalRarity = e.OriginalRarity;
            message.FinalRarity = e.FinalRarity;
        }
    }
}