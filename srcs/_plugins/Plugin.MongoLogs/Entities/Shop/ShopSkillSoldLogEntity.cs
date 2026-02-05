using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Shop;

namespace Plugin.MongoLogs.Entities.Shop
{
    [EntityFor(typeof(LogShopSkillSoldMessage))]
    [CollectionName(CollectionNames.SHOP_SKILL_SOLD, DisplayCollectionNames.SHOP_SKILL_SOLD)]
    public class ShopSkillSoldLogEntity : IPlayerLogEntity
    {
        public int SkillVnum { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}