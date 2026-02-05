using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.Game.Inventory.Event;

namespace Plugin.PlayerLogs.Enrichers.Inventory
{
    public class LogInventoryPickedUpPlayerItemMessageEnricher : ILogMessageEnricher<InventoryPickedUpPlayerItemEvent, LogInventoryPickedUpPlayerItemMessage>
    {
        public void Enrich(LogInventoryPickedUpPlayerItemMessage message, InventoryPickedUpPlayerItemEvent e)
        {
            message.Amount = e.Amount;
            message.Location = e.Location;
            message.ItemInstance = e.ItemInstance;
        }
    }
}