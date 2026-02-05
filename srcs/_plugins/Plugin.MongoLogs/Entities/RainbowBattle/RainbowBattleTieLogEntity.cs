using System;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.RainbowBattle;

namespace Plugin.MongoLogs.Entities.RainbowBattle
{
    [EntityFor(typeof(LogRainbowBattleTieMessage))]
    [CollectionName(CollectionNames.RAINBOW_BATTLE_TIE, DisplayCollectionNames.RAINBOW_BATTLE_MANAGEMENT_TIE)]
    public class RainbowBattleTieLogEntity : IPlayerLogEntity
    {
        public int[] RedTeam { get; set; }
        public int[] BlueTeam { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}