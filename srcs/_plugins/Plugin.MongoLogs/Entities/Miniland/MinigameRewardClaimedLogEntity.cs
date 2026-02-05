using System;
using MongoDB.Bson.Serialization.Attributes;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.Game.Configurations.Miniland;

namespace Plugin.MongoLogs.Entities.Miniland
{
    [EntityFor(typeof(LogMinigameRewardClaimedMessage))]
    [CollectionName(CollectionNames.MINIGAME_REWARDS_CLAIMED, DisplayCollectionNames.MINIGAME_REWARDS_CLAIMED)]
    internal class MinigameRewardClaimedLogEntity : IPlayerLogEntity
    {
        public long OwnerId { get; set; }
        public int MinigameVnum { get; set; }
        public string MinigameType { get; set; }
        public RewardLevel RewardLevel { get; set; }
        public bool Coupon { get; set; }
        public int ItemVnum { get; set; }
        public int Amount { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}