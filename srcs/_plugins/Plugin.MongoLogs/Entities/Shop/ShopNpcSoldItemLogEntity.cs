using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Shop
{
    [EntityFor(typeof(LogShopNpcSoldItemMessage))]
    [CollectionName(CollectionNames.SHOP_NPC_ITEM_SOLD, DisplayCollectionNames.SHOP_NPC_ITEM_SOLD)]
    public class ShopNpcSoldItemLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public long PricePerItem { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}