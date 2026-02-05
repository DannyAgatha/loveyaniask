using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Player;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogTradeRequestedMessage))]
    [CollectionName(CollectionNames.INVITATION_TRADE, DisplayCollectionNames.INVITATION_TRADE)]
    public class TradeRequestedLogEntity : IPlayerLogEntity
    {
        public long TargetId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}