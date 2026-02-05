using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidLostMessage))]
    [CollectionName(CollectionNames.RAID_ACTION_LOST, DisplayCollectionNames.RAID_ACTION_LOST)]
    public class RaidLostLogEntity : IPlayerLogEntity
    {
        public string RaidId { get; set; }
        public string RaidType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}