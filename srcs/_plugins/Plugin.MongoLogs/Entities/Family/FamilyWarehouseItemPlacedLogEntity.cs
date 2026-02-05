using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Family
{
    [EntityFor(typeof(LogFamilyWarehouseItemPlacedMessage))]
    [CollectionName(CollectionNames.FAMILY_WAREHOUSE_ITEM_PLACED, DisplayCollectionNames.FAMILY_WAREHOUSE_ITEM_PLACED)]
    public class FamilyWarehouseItemPlacedLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public short DestinationSlot { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}