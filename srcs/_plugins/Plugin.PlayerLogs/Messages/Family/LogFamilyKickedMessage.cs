using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Family
{
    [MessageType("logs.family.kicked")]
    public class LogFamilyKickedMessage : IPlayerActionLogMessage
    {
        public long FamilyId { get; set; }
        public long KickedMemberId { get; set; }
        public string KickedMemberName { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}