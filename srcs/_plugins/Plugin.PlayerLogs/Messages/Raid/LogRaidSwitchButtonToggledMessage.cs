using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Raid
{
    [MessageType("logs.raid.buttontoggled")]
    public class LogRaidSwitchButtonToggledMessage : IPlayerActionLogMessage
    {
        public Guid RaidId { get; set; }
        public long LeverId { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}