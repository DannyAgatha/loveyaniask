using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyJoinedMessage))]
    [CollectionName(CollectionNames.FAMILY_MANAGEMENT_JOINED, DisplayCollectionNames.FAMILY_MANAGEMENT_JOINED)]
    internal class FamilyJoinedLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public long InviterId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}