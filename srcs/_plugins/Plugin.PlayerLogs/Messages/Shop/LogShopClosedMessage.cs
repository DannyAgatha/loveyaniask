using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Helpers;

namespace Plugin.PlayerLogs.Messages.Shop
{
    [MessageType("logs.shop.closed")]
    public class LogShopClosedMessage : IPlayerActionLogMessage
    {
        public Location Location { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}