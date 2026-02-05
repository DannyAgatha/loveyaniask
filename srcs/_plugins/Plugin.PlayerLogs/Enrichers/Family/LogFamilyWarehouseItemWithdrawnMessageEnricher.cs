using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyWarehouseItemWithdrawnMessageEnricher : ILogMessageEnricher<FamilyWarehouseItemWithdrawnEvent, LogFamilyWarehouseItemWithdrawnMessage>
    {
        public void Enrich(LogFamilyWarehouseItemWithdrawnMessage message, FamilyWarehouseItemWithdrawnEvent e)
        {
            message.FamilyId = e.Sender.PlayerEntity.Family.Id;
            message.Amount = e.Amount;
            message.ItemInstance = e.ItemInstance;
            message.FromSlot = e.FromSlot;
        }
    }
}