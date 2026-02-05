using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Family
{
    [MessageType("logs.family.warehouse-item-placed")]
    public class LogFamilyWarehouseItemPlacedMessage : IPlayerActionLogMessage
    {
        public long FamilyId { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public short DestinationSlot { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}