using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.Game.Warehouse.Events;

namespace Plugin.PlayerLogs.Enrichers.Miniland
{
    public class LogWarehouseItemPlacedMessageEnricher : ILogMessageEnricher<WarehouseItemPlacedEvent, LogWarehouseItemPlacedMessage>
    {
        public void Enrich(LogWarehouseItemPlacedMessage message, WarehouseItemPlacedEvent e)
        {
            message.Amount = e.Amount;
            message.DestinationSlot = e.DestinationSlot;
            message.ItemInstance = e.ItemInstance;
        }
    }
}