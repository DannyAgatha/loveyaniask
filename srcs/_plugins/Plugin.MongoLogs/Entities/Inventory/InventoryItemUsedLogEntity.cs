using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Inventory;

namespace Plugin.MongoLogs.Entities.Inventory
{
    [EntityFor(typeof(LogInventoryItemUsedMessage))]
    [CollectionName(CollectionNames.INVENTORY_ITEM_USED, DisplayCollectionNames.INVENTORY_ITEM_USED)]
    public class InventoryItemUsedLogEntity : IPlayerLogEntity
    {
        public int ItemVnum { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}