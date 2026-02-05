using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogCellonUpgradedMessage))]
    [CollectionName(CollectionNames.UPGRADE_CELLON, DisplayCollectionNames.UPGRADE_CELLON)]
    internal class CellonUpgradedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO Item { get; set; }
        public int CellonVnum { get; set; }
        public bool Succeed { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}