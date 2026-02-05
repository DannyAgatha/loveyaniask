using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Raid
{
    [MessageType("logs.raid.targetkilled")]
    public class LogRaidTargetKilledMessage : IPlayerActionLogMessage
    {
        public Guid RaidId { get; set; }
        public long[] DamagerCharactersIds { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}