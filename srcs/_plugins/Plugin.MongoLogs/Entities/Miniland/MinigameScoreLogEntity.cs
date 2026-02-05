using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Miniland;

namespace Plugin.MongoLogs.Entities.Miniland
{
    [EntityFor(typeof(LogMinigameScoreMessage))]
    [CollectionName(CollectionNames.MINIGAME_SCORE, DisplayCollectionNames.MINIGAME_SCORE)]
    internal class MinigameScoreLogEntity : IPlayerLogEntity
    {
        public string CharacterName { get; set; }
        public long OwnerId { get; set; }
        public TimeSpan CompletionTime { get; set; }
        public int MinigameVnum { get; set; }
        public string MinigameType { get; set; }
        public long Score1 { get; set; }
        public long Score2 { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}