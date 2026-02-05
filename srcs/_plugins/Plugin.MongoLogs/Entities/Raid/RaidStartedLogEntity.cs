using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidStartedMessage))]
    [CollectionName(CollectionNames.RAID_MANAGEMENT_STARTED, DisplayCollectionNames.RAID_MANAGEMENT_STARTED)]
    public class RaidStartedLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public long[] MembersIds { get; set; }
        public string RaidType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}