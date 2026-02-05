using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Player;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogGroupInvitedMessage))]
    [CollectionName(CollectionNames.INVITATION_GROUP, DisplayCollectionNames.INVITATION_GROUP)]
    public class GroupInvitedLogEntity : IPlayerLogEntity
    {
        public long GroupId { get; set; }
        public long TargetId { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}