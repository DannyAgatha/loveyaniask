using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Family
{
    [MessageType("logs.family.message")]
    public class LogFamilyMessageMessage : IPlayerActionLogMessage
    {
        public string FamilyMessageType { get; set; }
        public long FamilyId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}