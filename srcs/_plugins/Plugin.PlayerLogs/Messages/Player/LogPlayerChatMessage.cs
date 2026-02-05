// NosEmu
// 


using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._playerActionLogs;

namespace Plugin.PlayerLogs.Messages.Player
{
    [MessageType("logs.chat.message")]
    public class LogPlayerChatMessage : IPlayerActionLogMessage
    {
        public ChatType ChatType { get; set; }
        public long? TargetCharacterId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}