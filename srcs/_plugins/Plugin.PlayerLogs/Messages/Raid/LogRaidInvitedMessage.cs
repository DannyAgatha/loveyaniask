using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Raid
{
    [MessageType("logs.raid.invited")]
    public class LogRaidInvitedMessage : IPlayerActionLogMessage
    {
        public Guid RaidId { get; set; }
        public long TargetId { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}