using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Quest;

namespace Plugin.MongoLogs.Entities.Quest
{
    [EntityFor(typeof(LogQuestAddedMessage))]
    [CollectionName(CollectionNames.QUEST_ADDED, DisplayCollectionNames.QUEST_ADDED)]
    internal class QuestAddedLogEntity : IPlayerLogEntity
    {
        public int QuestId { get; set; }
        public string SlotType { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}