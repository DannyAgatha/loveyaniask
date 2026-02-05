using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Mail;
using WingsEmu.DTOs.Items;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogMailRemovedMessage))]
    [CollectionName(CollectionNames.MAIL_REMOVED, DisplayCollectionNames.MAIL_REMOVED)]
    public class MailRemovedLogEntity : IPlayerLogEntity
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