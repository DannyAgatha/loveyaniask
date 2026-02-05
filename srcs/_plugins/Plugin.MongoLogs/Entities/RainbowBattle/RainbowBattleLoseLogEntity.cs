using System;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.RainbowBattle;

namespace Plugin.MongoLogs.Entities.RainbowBattle
{
    [EntityFor(typeof(LogRainbowBattleLoseMessage))]
    [CollectionName(CollectionNames.RAINBOW_BATTLE_LOSE, DisplayCollectionNames.RAINBOW_BATTLE_MANAGEMENT_LOSE)]
    public class RainbowBattleLoseLogEntity : IPlayerLogEntity
    {
        public Guid RainbowBattleId { get; set; }
        public int[] Players { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}