using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Bazaar
{
    [MessageType("logs.bazaar.iteminserted")]
    public class LogBazaarItemInsertedMessage : IPlayerActionLogMessage
    {
        public long BazaarItemId { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }
        public long Taxes { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}