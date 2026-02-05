using System;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.RainbowBattle;

namespace Plugin.MongoLogs.Entities.RainbowBattle
{
    [EntityFor(typeof(LogRainbowBattleJoinMessage))]
    [CollectionName(CollectionNames.RAINBOW_BATTLE_JOIN, DisplayCollectionNames.RAINBOW_BATTLE_MANAGEMENT_JOIN)]
    public class RainbowBattleJoinLogEntity : IPlayerLogEntity
    {
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}