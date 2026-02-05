using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Bazaar
{
    [EntityFor(typeof(LogBazaarItemBoughtMessage))]
    [CollectionName(CollectionNames.BAZAAR_ITEM_BOUGHT, DisplayCollectionNames.BAZAAR_ITEM_BOUGHT)]
    public class BazaarItemBoughtLogEntity : IPlayerLogEntity
    {
        public long BazaarItemId { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public long PricePerItem { get; set; }
        public int Amount { get; set; }
        public ItemInstanceDTO BoughtItem { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}