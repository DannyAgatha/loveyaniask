using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Bazaar;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Bazaar
{
    [EntityFor(typeof(LogBazaarItemWithdrawnMessage))]
    [CollectionName(CollectionNames.BAZAAR_ITEM_WITHDRAWN, DisplayCollectionNames.BAZAAR_ITEM_WITHDRAWN)]
    public class BazaarItemWithdrawnLogEntity : IPlayerLogEntity
    {
        public long BazaarItemId { get; set; }
        public long Price { get; set; }
        public int Quantity { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public long ClaimedMoney { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}