using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Quest;

namespace Plugin.MongoLogs.Entities.Quest
{
    [EntityFor(typeof(LogQuestAbandonedMessage))]
    [CollectionName(CollectionNames.QUEST_ABANDONED, DisplayCollectionNames.QUEST_ABANDONED)]
    internal class QuestAbandonedLogEntity : IPlayerLogEntity
    {
        public int QuestId { get; set; }
        public string SlotType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}