using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Inventory
{
    [EntityFor(typeof(LogInventoryPickedUpPlayerItemMessage))]
    [CollectionName(CollectionNames.INVENTORY_PICKED_UP_PLAYER_ITEM, DisplayCollectionNames.INVENTORY_PICKED_UP_PLAYER_ITEM)]
    public class InventoryPickedUpPlayerItemLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; init; }
        public Location Location { get; init; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}