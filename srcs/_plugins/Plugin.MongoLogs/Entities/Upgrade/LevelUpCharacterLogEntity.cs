using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.LevelUp;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Upgrade
{
    [EntityFor(typeof(LogLevelUpCharacterMessage))]
    [CollectionName(CollectionNames.LEVEL_UP_CHARACTER, DisplayCollectionNames.LEVEL_UP_CHARACTER)]
    internal class LevelUpCharacterLogEntity : IPlayerLogEntity
    {
        public int Level { get; set; }
        public string LevelType { get; set; }
        public Location Location { get; set; }
        public int? ItemVnum { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}