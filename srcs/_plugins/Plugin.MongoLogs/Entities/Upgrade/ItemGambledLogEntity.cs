using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogItemGambledMessage))]
    [CollectionName(CollectionNames.UPGRADE_ITEM_GAMBLED, DisplayCollectionNames.UPGRADE_ITEM_GAMBLED)]
    internal class ItemGambledLogEntity : IPlayerLogEntity
    {
        public int ItemVnum { get; set; }
        public string Mode { get; set; }
        public string Protection { get; set; }
        public int? Amulet { get; set; }
        public bool Succeed { get; set; }
        public short OriginalRarity { get; set; }
        public short? FinalRarity { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}