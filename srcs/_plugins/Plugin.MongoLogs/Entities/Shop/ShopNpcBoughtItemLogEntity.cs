using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Shop
{
    [EntityFor(typeof(LogShopNpcBoughtItemMessage))]
    [CollectionName(CollectionNames.SHOP_NPC_ITEM_BOUGHT, DisplayCollectionNames.SHOP_NPC_ITEM_BOUGHT)]
    public class ShopNpcBoughtItemLogEntity : IPlayerLogEntity
    {
        public long SellerId { get; set; }
        public long TotalPrice { get; set; }
        public int Quantity { get; set; }
        public string CurrencyType { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}