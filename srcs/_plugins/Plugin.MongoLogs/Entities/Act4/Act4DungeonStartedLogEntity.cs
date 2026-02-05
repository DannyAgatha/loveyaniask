using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Act4;

namespace Plugin.MongoLogs.Entities.Act4
{
    [EntityFor(typeof(LogAct4DungeonStartedMessage))]
    [CollectionName(CollectionNames.ACT4_DUNGEON_STARTED, DisplayCollectionNames.ACT4_DUNGEON_STARTED)]
    public class Act4DungeonStartedLogEntity : IPlayerLogEntity
    {
        public string FactionType { get; set; }
        public string DungeonType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}