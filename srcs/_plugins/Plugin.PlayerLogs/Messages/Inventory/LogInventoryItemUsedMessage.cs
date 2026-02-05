using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Inventory
{
    [MessageType("logs.inventory.item-used")]
    public class LogInventoryItemUsedMessage : IPlayerActionLogMessage
    {
        public int ItemVnum { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}