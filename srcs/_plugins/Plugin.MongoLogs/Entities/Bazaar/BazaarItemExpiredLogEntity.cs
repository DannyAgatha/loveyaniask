using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Bazaar
{
    [EntityFor(typeof(LogBazaarItemExpiredMessage))]
    [CollectionName(CollectionNames.BAZAAR_ITEM_EXPIRED, DisplayCollectionNames.BAZAAR_ITEM_EXPIRED)]
    public class BazaarItemExpiredLogEntity : IPlayerLogEntity
    {
        public long BazaarItemId { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }
        public ItemInstanceDTO Item { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}