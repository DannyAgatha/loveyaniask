using System;
using Plugin.MongoLogs.Utils;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using WingsEmu.Game.RainbowBattle;

namespace Plugin.MongoLogs.Entities.RainbowBattle
{
    [EntityFor(typeof(LogRainbowBattleFrozenMessage))]
    [CollectionName(CollectionNames.RAINBOW_BATTLE_FROZEN, DisplayCollectionNames.RAINBOW_BATTLE_MANAGEMENT_FROZEN)]
    public class RainbowBattleFrozenLogEntity : IPlayerLogEntity
    {
        public Guid RainbowBattleId { get; set; }
        public RainbowBattlePlayerDump Killer { get; set; }
        public RainbowBattlePlayerDump Killed { get; set; }
        public long CharacterId { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}