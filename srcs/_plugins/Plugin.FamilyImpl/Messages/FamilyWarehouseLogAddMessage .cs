using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.warehouse.log.add")]
    public class FamilyWarehouseLogAddMessage : IMessage
    {
        public long FamilyId { get; set; }

        public FamilyWarehouseLogEntryDto LogToAdd { get; set; }
    }
}