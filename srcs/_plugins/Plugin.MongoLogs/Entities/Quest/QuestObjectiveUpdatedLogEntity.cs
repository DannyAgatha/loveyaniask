using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Quest;

namespace Plugin.MongoLogs.Entities.Quest
{
    [EntityFor(typeof(LogQuestObjectiveUpdatedMessage))]
    [CollectionName(CollectionNames.QUEST_OBJECTIVE_UPDATED, DisplayCollectionNames.QUEST_OBJECTIVE_UPDATED)]
    internal class QuestObjectiveUpdatedLogEntity : IPlayerLogEntity
    {
        public int QuestId { get; set; }
        public string SlotType { get; set; }

        public Dictionary<string, int> UpdatedObjectivesAmount { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}