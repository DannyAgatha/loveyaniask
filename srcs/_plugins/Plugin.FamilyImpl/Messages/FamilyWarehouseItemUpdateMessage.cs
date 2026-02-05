using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.warehouse.item.update")]
    public class FamilyWarehouseItemUpdateMessage : IMessage
    {
        public long FamilyId { get; set; }

        public IEnumerable<(FamilyWarehouseItemDto dto, short slot)> UpdatedItems { get; set; }
    }
}