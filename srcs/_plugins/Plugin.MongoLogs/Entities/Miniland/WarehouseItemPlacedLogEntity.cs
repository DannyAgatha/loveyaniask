using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Miniland
{
    [EntityFor(typeof(LogWarehouseItemPlacedMessage))]
    [CollectionName(CollectionNames.WAREHOUSE_ITEM_PLACED, DisplayCollectionNames.WAREHOUSE_ITEM_PLACED)]
    public class WarehouseItemPlacedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public short DestinationSlot { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}