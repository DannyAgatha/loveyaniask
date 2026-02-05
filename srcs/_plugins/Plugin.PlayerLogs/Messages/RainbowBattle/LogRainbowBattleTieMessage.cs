using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.RainbowBattle
{
    [MessageType("logs.rainbowbattle.tie")]
    public class LogRainbowBattleTieMessage : IPlayerActionLogMessage
    {
        public int[] RedTeam { get; set; }
        public int[] BlueTeam { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}