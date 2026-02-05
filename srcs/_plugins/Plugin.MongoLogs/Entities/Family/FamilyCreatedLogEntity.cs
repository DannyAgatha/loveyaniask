using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyCreatedMessage))]
    [CollectionName(CollectionNames.FAMILY_MANAGEMENT_CREATED, DisplayCollectionNames.FAMILY_MANAGEMENT_CREATED)]
    internal class FamilyCreatedLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public string FamilyName { get; set; }
        public List<long> DeputiesIds { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}