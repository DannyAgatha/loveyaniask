using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyMessageMessage))]
    [CollectionName(CollectionNames.FAMILY_MANAGEMENT_MESSAGES, DisplayCollectionNames.FAMILY_MANAGEMENT_MESSAGES)]
    internal class FamilyMessageLogEntity : IPlayerLogEntity
    {
        public string FamilyMessageType { get; set; }
        public long FamilyId { get; set; }
        public string Message { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}