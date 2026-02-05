using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogItemSummedMessage))]
    [CollectionName(CollectionNames.UPGRADE_RESISTANCE_SUMMED, DisplayCollectionNames.UPGRADE_RESISTANCE_SUMMED)]
    internal class ItemSummedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO LeftItem { get; set; }
        public ItemInstanceDTO RightItem { get; set; }
        public bool Succeed { get; set; }
        public int SumLevel { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}