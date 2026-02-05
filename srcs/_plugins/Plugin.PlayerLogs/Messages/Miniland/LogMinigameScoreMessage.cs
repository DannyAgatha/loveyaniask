using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Miniland
{
    [MessageType("logs.minigame.score")]
    public class LogMinigameScoreMessage : IPlayerActionLogMessage
    {
        public long OwnerId { get; set; }
        public TimeSpan CompletionTime { get; set; }
        public int MinigameVnum { get; set; }
        public string MinigameType { get; set; }
        public long Score1 { get; set; }
        public long Score2 { get; set; }
        public string ScoreValidity { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}