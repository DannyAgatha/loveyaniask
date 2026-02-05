using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Bazaar
{
    [MessageType("logs.bazaar.boughtitems")]
    public class LogBazaarItemBoughtMessage : IPlayerActionLogMessage
    {
        public long BazaarItemId { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public long PricePerItem { get; set; }
        public int Amount { get; set; }
        public ItemInstanceDTO BoughtItem { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}