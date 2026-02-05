using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Inventory
{
    [EntityFor(typeof(LogInventoryPickedUpItemMessage))]
    [CollectionName(CollectionNames.INVENTORY_PICKED_UP_ITEM, DisplayCollectionNames.INVENTORY_PICKED_UP_ITEM)]
    public class InventoryPickedUpItemLogEntity : IPlayerLogEntity
    {
        public int ItemVnum { get; set; }
        public int Amount { get; set; }
        public Location Location { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}