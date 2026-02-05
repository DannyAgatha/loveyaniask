using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Mail;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogMailClaimedMessage))]
    [CollectionName(CollectionNames.MAIL_CLAIMED, DisplayCollectionNames.MAIL_CLAIMED)]
    public class MailClaimedLogEntity : IPlayerLogEntity
    {
        public long MailId { get; set; }
        public string SenderName { get; set; }
        public ItemInstanceDTO ItemInstance { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}