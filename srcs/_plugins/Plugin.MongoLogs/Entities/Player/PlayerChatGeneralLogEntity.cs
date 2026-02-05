using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Player;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogPlayerChatMessage))]
    [CollectionName(CollectionNames.CHAT, DisplayCollectionNames.CHAT)]
    internal class PlayerChatGeneralLogEntity : IPlayerLogEntity
    {
        public long? TargetCharacterId { get; set; }
        public string Message { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}