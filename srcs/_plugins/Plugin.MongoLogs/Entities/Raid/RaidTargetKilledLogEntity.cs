using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidTargetKilledMessage))]
    [CollectionName(CollectionNames.RAID_ACTION_TARGET_KILLED, DisplayCollectionNames.RAID_ACTION_TARGET_KILLED)]
    public class RaidTargetKilledLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public long[] DamagerCharactersIds { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}