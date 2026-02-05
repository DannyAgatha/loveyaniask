using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidCreatedMessage))]
    [CollectionName(CollectionNames.RAID_MANAGEMENT_CREATED, DisplayCollectionNames.RAID_MANAGEMENT_CREATED)]
    public class RaidCreatedLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public string RaidType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}