using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Raid;

namespace Plugin.MongoLogs.Entities.Raid
{
    [EntityFor(typeof(LogRaidInvitedMessage))]
    [CollectionName(CollectionNames.INVITATION_RAID, DisplayCollectionNames.INVITATION_RAID)]
    public class RaidInvitedLogEntity : IPlayerLogEntity
    {
        public Guid RaidId { get; set; }
        public long TargetId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}