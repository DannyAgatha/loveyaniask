using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Shop;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Shop
{
    [EntityFor(typeof(LogShopOpenedMessage))]
    [CollectionName(CollectionNames.SHOP_OPENED, DisplayCollectionNames.SHOP_OPENED)]
    public class ShopOpenedLogEntity : IPlayerLogEntity
    {
        public string ShopName { get; set; }
        public Location Location { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}