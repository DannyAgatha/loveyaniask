using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgraded.gambled")]
    public class LogItemGambledMessage : IPlayerActionLogMessage
    {
        public int ItemVnum { get; set; }
        public string Mode { get; set; }
        public string Protection { get; set; }
        public int? Amulet { get; set; }
        public bool Succeed { get; set; }
        public short OriginalRarity { get; set; }
        public short? FinalRarity { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}