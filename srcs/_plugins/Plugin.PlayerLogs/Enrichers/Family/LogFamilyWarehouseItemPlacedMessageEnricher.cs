using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyWarehouseItemPlacedMessageEnricher : ILogMessageEnricher<FamilyWarehouseItemPlacedEvent, LogFamilyWarehouseItemPlacedMessage>
    {
        public void Enrich(LogFamilyWarehouseItemPlacedMessage message, FamilyWarehouseItemPlacedEvent e)
        {
            message.FamilyId = e.Sender.PlayerEntity.Family.Id;
            message.Amount = e.Amount;
            message.ItemInstance = e.ItemInstance;
            message.DestinationSlot = e.DestinationSlot;
        }
    }
}