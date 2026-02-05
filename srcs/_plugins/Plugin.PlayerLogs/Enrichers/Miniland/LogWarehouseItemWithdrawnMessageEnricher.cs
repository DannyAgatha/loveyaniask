using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.Game.Warehouse.Events;

namespace Plugin.PlayerLogs.Enrichers.Miniland
{
    public class LogWarehouseItemWithdrawnMessageEnricher : ILogMessageEnricher<WarehouseItemWithdrawnEvent, LogWarehouseItemWithdrawnMessage>
    {
        public void Enrich(LogWarehouseItemWithdrawnMessage message, WarehouseItemWithdrawnEvent e)
        {
            message.Amount = e.Amount;
            message.FromSlot = e.FromSlot;
            message.ItemInstance = e.ItemInstance;
        }
    }
}