using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Miniland
{
    [MessageType("logs.warehouse.item-withdrawn")]
    public class LogWarehouseItemWithdrawnMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public short FromSlot { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}