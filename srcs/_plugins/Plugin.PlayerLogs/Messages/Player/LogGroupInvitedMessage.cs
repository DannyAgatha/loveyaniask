using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Player
{
    [MessageType("logs.group.invited")]
    public class LogGroupInvitedMessage : IPlayerActionLogMessage
    {
        public long GroupId { get; set; }
        public long TargetId { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}