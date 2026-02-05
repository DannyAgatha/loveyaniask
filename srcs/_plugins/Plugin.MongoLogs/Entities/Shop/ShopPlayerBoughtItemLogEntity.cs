using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Shop
{
    [EntityFor(typeof(LogShopPlayerBoughtItemMessage))]
    [CollectionName(CollectionNames.SHOP_PLAYER_ITEM_BOUGHT, DisplayCollectionNames.SHOP_PLAYER_ITEM_BOUGHT)]
    public class ShopPlayerBoughtItemLogEntity : IPlayerLogEntity
    {
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public long TotalPrice { get; set; }
        public int Quantity { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}