using System;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.RainbowBattle;

namespace Plugin.MongoLogs.Entities.RainbowBattle
{
    [EntityFor(typeof(LogRainbowBattleWonMessage))]
    [CollectionName(CollectionNames.RAINBOW_BATTLE_WON, DisplayCollectionNames.RAINBOW_BATTLE_MANAGEMENT_WON)]
    public class RainbowBattleWonLogEntity : IPlayerLogEntity
    {
        public Guid RainbowBattleId { get; set; }
        public int[] Players { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}