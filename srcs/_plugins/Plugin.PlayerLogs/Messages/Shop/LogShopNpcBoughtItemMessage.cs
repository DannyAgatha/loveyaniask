using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Shop
{
    [MessageType("logs.shop.npcitembought")]
    public class LogShopNpcBoughtItemMessage : IPlayerActionLogMessage
    {
        public long SellerId { get; set; }
        public long TotalPrice { get; set; }
        public int Quantity { get; set; }
        public string CurrencyType { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}