using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Player;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogPlayerDisconnectedMessage))]
    [CollectionName(CollectionNames.CONNECTION_SESSION, DisplayCollectionNames.CONNECTION_SESSION)]
    public class PlayerDisconnectedLogEntity : IPlayerLogEntity
    {
        public int ChannelId { get; set; }

        public string HardwareId { get; set; }
        public string MasterAccountId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime SessionStart { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime SessionEnd { get; set; }

        public TimeSpan SessionDuration { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}