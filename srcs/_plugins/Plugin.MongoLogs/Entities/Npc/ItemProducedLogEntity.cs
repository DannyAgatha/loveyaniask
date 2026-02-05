using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Npc;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Npc
{
    [EntityFor(typeof(LogItemProducedMessage))]
    [CollectionName(CollectionNames.NPC_ITEM_PRODUCED, DisplayCollectionNames.NPC_ITEM_PRODUCED)]
    public class ItemProducedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int ItemAmount { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}