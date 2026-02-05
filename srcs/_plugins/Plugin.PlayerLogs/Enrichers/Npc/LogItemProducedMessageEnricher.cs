using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Npc;
using WingsEmu.Game.Npcs.Event;

namespace Plugin.PlayerLogs.Enrichers.Npc
{
    public class LogItemProducedMessageEnricher : ILogMessageEnricher<ItemProducedEvent, LogItemProducedMessage>
    {
        public void Enrich(LogItemProducedMessage message, ItemProducedEvent e)
        {
            message.ItemAmount = e.ItemAmount;
            message.ItemInstance = e.ItemInstance;
        }
    }
}