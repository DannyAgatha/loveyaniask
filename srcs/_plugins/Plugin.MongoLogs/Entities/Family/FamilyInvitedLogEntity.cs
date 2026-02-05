using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyInvitedMessage))]
    [CollectionName(CollectionNames.INVITATION_FAMILY, DisplayCollectionNames.INVITATION_FAMILY)]
    public class FamilyInvitedLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public long TargetId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}