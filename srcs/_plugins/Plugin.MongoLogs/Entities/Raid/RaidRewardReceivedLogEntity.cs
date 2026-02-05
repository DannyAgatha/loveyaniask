using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidRewardReceivedMessage))]
    [CollectionName(CollectionNames.RAID_ACTION_REWARD_RECEIVED, DisplayCollectionNames.RAID_ACTION_REWARD_RECEIVED)]
    public class RaidRewardReceivedLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public byte BoxRarity { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}