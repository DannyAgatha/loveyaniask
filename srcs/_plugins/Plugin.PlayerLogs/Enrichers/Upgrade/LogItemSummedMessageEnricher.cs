using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogItemSummedMessageEnricher : ILogMessageEnricher<ItemSummedEvent, LogItemSummedMessage>
    {
        public void Enrich(LogItemSummedMessage message, ItemSummedEvent e)
        {
            message.LeftItem = e.LeftItem;
            message.RightItem = e.RightItem;
            message.Succeed = e.Succeed;
            message.SumLevel = e.SumLevel;
        }
    }
}