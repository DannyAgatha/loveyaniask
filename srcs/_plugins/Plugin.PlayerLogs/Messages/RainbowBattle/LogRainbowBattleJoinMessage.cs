using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.RainbowBattle
{
    [MessageType("logs.rainbowbattle.join")]
    public class LogRainbowBattleJoinMessage : IPlayerActionLogMessage
    {
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}