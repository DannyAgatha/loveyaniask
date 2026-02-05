using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages
{
    [MessageType("logs.strange.behavior")]
    public class LogStrangeBehaviorMessage : IPlayerActionLogMessage
    {
        public string SeverityType { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}