using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.Game.Inventory.Event;

namespace Plugin.PlayerLogs.Enrichers.Inventory
{
    public class LogInventoryItemUsedMessageEnricher : ILogMessageEnricher<InventoryItemUsedEvent, LogInventoryItemUsedMessage>
    {
        public void Enrich(LogInventoryItemUsedMessage message, InventoryItemUsedEvent e)
        {
            message.ItemVnum = e.ItemVnum;
        }
    }
}