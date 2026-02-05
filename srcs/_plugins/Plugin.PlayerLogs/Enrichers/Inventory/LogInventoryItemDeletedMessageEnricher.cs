using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.Game.Inventory.Event;

namespace Plugin.PlayerLogs.Enrichers.Inventory
{
    public class LogInventoryItemDeletedMessageEnricher : ILogMessageEnricher<InventoryItemDeletedEvent, LogInventoryItemDeletedMessage>
    {
        public void Enrich(LogInventoryItemDeletedMessage message, InventoryItemDeletedEvent e)
        {
            message.ItemAmount = e.ItemAmount;
            message.ItemInstance = e.ItemInstance;
        }
    }
}