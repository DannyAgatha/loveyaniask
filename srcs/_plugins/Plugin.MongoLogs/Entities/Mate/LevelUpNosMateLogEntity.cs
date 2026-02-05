using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.LevelUp;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Mate
{
    [EntityFor(typeof(LogLevelUpNosMateMessage))]
    [CollectionName(CollectionNames.LEVEL_UP_NOSMATE, DisplayCollectionNames.LEVEL_UP_NOSMATE)]
    internal class LevelUpNosMateLogEntity : IPlayerLogEntity
    {
        public int Level { get; set; }
        public string LevelUpType { get; set; }
        public Location Location { get; set; }
        public int? ItemVnum { get; set; }
        public int NosMateMonsterVnum { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}