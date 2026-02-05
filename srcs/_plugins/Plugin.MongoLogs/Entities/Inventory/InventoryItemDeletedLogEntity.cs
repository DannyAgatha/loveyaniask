using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Inventory
{
    [EntityFor(typeof(LogInventoryItemDeletedMessage))]
    [CollectionName(CollectionNames.INVENTORY_ITEM_DELETED, DisplayCollectionNames.INVENTORY_ITEM_DELETED)]
    public class InventoryItemDeletedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int ItemAmount { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}