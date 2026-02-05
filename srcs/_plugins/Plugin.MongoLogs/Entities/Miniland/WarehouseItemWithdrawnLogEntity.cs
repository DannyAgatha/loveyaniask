using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Miniland
{
    [EntityFor(typeof(LogWarehouseItemWithdrawnMessage))]
    [CollectionName(CollectionNames.WAREHOUSE_ITEM_WITHDRAWN, DisplayCollectionNames.WAREHOUSE_ITEM_WITHDRAWN)]
    public class WarehouseItemWithdrawnLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public short FromSlot { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}