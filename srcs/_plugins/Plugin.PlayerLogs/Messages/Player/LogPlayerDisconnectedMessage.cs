using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Player
{
    [MessageType("logs.connection.disconnected")]
    public class LogPlayerDisconnectedMessage : IPlayerActionLogMessage
    {
        public string HardwareId { get; set; }
        public string MasterAccountId { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }
        public TimeSpan SessionDuration => SessionEnd - SessionStart;
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}