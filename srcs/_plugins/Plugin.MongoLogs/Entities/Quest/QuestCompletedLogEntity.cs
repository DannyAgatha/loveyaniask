using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Quest;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Quest
{
    [EntityFor(typeof(LogQuestCompletedMessage))]
    [CollectionName(CollectionNames.QUEST_COMPLETED, DisplayCollectionNames.QUEST_COMPLETED)]
    internal class QuestCompletedLogEntity : IPlayerLogEntity
    {
        public int QuestId { get; set; }
        public string SlotType { get; set; }
        public Location Location { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}