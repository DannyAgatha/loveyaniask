using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.Game.Inventory.Event;

namespace Plugin.PlayerLogs.Enrichers.Inventory
{
    public class LogInventoryPickedUpItemMessageEnricher : ILogMessageEnricher<InventoryPickedUpItemEvent, LogInventoryPickedUpItemMessage>
    {
        public void Enrich(LogInventoryPickedUpItemMessage message, InventoryPickedUpItemEvent e)
        {
            message.Amount = e.Amount;
            message.ItemVnum = e.ItemVnum;
            message.Location = e.Location;
        }
    }
}