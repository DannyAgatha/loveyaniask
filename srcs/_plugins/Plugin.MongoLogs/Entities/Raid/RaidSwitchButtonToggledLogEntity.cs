using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidSwitchButtonToggledMessage))]
    [CollectionName(CollectionNames.RAID_ACTION_LEVER_ACTIVATED, DisplayCollectionNames.RAID_ACTION_LEVER_ACTIVATED)]
    public class RaidSwitchButtonToggledLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public long LeverId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}