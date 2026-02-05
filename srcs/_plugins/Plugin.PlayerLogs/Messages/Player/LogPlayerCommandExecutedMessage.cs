using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Player
{
    [MessageType("logs.player.commands")]
    public class LogPlayerCommandExecutedMessage : IPlayerActionLogMessage
    {
        public string Command { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}