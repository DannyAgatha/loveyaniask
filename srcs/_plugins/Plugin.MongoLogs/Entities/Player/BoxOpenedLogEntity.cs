using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogBoxOpenedMessage))]
    [CollectionName(CollectionNames.RANDOM_BOX_OPENED, DisplayCollectionNames.RANDOM_BOX_OPENED)]
    public class BoxOpenedLogEntity : IPlayerLogEntity
    {
        public ItemInstanceDTO Box { get; set; }
        public List<ItemInstanceDTO> Rewards { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}