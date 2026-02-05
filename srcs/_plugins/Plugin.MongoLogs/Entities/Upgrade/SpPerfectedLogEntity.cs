using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogSpPerfectedMessage))]
    [CollectionName(CollectionNames.UPGRADE_SP_PERFECTED, DisplayCollectionNames.UPGRADE_SP_PERFECTED)]
    public class SpPerfectedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO Sp { get; set; }
        public bool Success { get; set; }
        public byte SpPerfectionLevel { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}