using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.RainbowBattle
{
    [MessageType("logs.rainbowbattle.lose")]
    public class LogRainbowBattleLoseMessage : IPlayerActionLogMessage
    {
        public Guid RainbowBattleId { get; set; }
        public int[] Players { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}