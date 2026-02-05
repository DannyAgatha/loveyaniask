using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Act4;

namespace Plugin.MongoLogs.Entities.Act4
{
    [EntityFor(typeof(LogAct4PvpKillMessage))]
    [CollectionName(CollectionNames.ACT4_KILL, DisplayCollectionNames.ACT4_KILL)]
    public class Act4PvpKillLogEntity : IPlayerLogEntity
    {
        public long TargetId { get; set; }
        public string KillerFaction { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}