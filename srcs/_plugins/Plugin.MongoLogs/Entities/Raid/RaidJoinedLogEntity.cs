using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidJoinedMessage))]
    [CollectionName(CollectionNames.RAID_MANAGEMENT_JOINED, DisplayCollectionNames.RAID_MANAGEMENT_JOINED)]
    public class RaidJoinedLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public string RaidJoinType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}