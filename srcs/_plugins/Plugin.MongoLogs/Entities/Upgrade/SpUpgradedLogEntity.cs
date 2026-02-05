using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogSpUpgradedMessage))]
    [CollectionName(CollectionNames.UPGRADE_SP_UPGRADED, DisplayCollectionNames.UPGRADE_SP_UPGRADED)]
    internal class SpUpgradedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO Sp { get; set; }
        public string Mode { get; set; }
        public string Result { get; set; }
        public short OriginalUpgrade { get; set; }
        public bool IsProtected { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}