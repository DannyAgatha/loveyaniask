using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Raid
{
    [MessageType("logs.raid.lost")]
    public class LogRaidLostMessage : IPlayerActionLogMessage
    {
        public string RaidId { get; set; }
        public string RaidType { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}