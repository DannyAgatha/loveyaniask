using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Shop
{
    [MessageType("logs.shop.npc-item-sold")]
    public class LogShopNpcSoldItemMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public long PricePerItem { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}