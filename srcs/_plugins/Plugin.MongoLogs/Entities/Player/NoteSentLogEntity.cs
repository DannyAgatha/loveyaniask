using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Mail;

namespace Plugin.MongoLogs.Entities.Player
{
    [EntityFor(typeof(LogNoteSentMessage))]
    [CollectionName(CollectionNames.NOTE_SENT, DisplayCollectionNames.NOTE_SENT)]
    public class NoteSentLogEntity : IPlayerLogEntity
    {
        public long NoteId { get; set; }
        public string ReceiverName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}