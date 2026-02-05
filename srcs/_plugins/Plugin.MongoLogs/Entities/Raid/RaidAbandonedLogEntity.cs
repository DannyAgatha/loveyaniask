using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidAbandonedMessage))]
    [CollectionName(CollectionNames.RAID_MANAGEMENT_ABANDONED, DisplayCollectionNames.RAID_MANAGEMENT_ABANDONED)]
    public class RaidAbandonedLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}