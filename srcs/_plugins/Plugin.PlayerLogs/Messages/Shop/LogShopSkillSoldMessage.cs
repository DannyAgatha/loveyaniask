using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Shop
{
    [MessageType("logs.shop.skill-sold")]
    public class LogShopSkillSoldMessage : IPlayerActionLogMessage
    {
        public int SkillVnum { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}