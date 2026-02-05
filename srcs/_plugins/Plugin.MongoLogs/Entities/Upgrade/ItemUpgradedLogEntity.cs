using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogItemUpgradedMessage))]
    [CollectionName(CollectionNames.UPGRADE_ITEM_UPGRADED, DisplayCollectionNames.UPGRADE_ITEM_UPGRADED)]
    internal class ItemUpgradedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO Item { get; set; }
        public long TotalPrice { get; set; }
        public string Mode { get; set; }
        public string Protection { get; set; }
        public bool HasAmulet { get; set; }
        public short OriginalUpgrade { get; set; }
        public string Result { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}