using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyUpgradeBoughtMessage))]
    [CollectionName(CollectionNames.FAMILY_MANAGEMENT_UPGRADE_BOUGHT, DisplayCollectionNames.FAMILY_MANAGEMENT_UPGRADE_BOUGHT)]
    public class FamilyUpgradeBoughtLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public int UpgradeVnum { get; set; }
        public string FamilyUpgradeType { get; set; }
        public short UpgradeValue { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}