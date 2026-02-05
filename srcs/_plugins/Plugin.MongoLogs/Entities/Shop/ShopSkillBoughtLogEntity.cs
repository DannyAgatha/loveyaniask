using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Shop;

namespace Plugin.MongoLogs.Entities.Shop
{
    [EntityFor(typeof(LogShopSkillBoughtMessage))]
    [CollectionName(CollectionNames.SHOP_SKILL_BOUGHT, DisplayCollectionNames.SHOP_SKILL_BOUGHT)]
    public class ShopSkillBoughtLogEntity : IPlayerLogEntity
    {
        public short SkillVnum { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}