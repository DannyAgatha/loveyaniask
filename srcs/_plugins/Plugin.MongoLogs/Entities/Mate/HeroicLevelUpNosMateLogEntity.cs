using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.HeroicLevelUp;
using System;
using WingsEmu.Game.Helpers;

namespace Plugin.MongoLogs.Entities.Mate
{
    [EntityFor(typeof(LogHeroicLevelUpNosMateMessage))]
    [CollectionName(CollectionNames.HEROIC_LEVEL_UP_NOSMATE, DisplayCollectionNames.HEROIC_LEVEL_UP_NOSMATE)]
    internal class HeroicLevelUpNosMateLogEntity : IPlayerLogEntity
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