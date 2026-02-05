using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyLeftMessage))]
    [CollectionName(CollectionNames.FAMILY_MANAGEMENT_LEFT, DisplayCollectionNames.FAMILY_MANAGEMENT_LEFT)]
    internal class FamilyLeftLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}