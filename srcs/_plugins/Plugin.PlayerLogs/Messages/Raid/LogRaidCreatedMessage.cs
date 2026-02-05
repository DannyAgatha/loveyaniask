using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Raid
{
    [MessageType("logs.raid.created")]
    public class LogRaidCreatedMessage : IPlayerActionLogMessage
    {
        public Guid RaidId { get; set; }
        public string RaidType { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}