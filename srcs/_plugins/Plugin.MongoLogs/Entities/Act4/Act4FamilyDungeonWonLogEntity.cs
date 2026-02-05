using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Act4;
using WingsEmu.Game.Act4;

namespace Plugin.MongoLogs.Entities.Act4
{
    [EntityFor(typeof(LogAct4FamilyDungeonWonMessage))]
    [CollectionName(CollectionNames.ACT4_DUNGEON_WON, DisplayCollectionNames.ACT4_DUNGEON_WON)]
    public class Act4FamilyDungeonWonLogEntity : IPlayerLogEntity
    {
        public long FamilyId { get; set; }
        public DungeonType DungeonType { get; set; }
        public IEnumerable<int> DungeonMembers { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}