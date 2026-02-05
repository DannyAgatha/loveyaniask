using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.RainbowBattle;

namespace Plugin.PlayerLogs.Messages.RainbowBattle
{
    [MessageType("logs.rainbowbattle.frozen")]
    public class LogRainbowBattleFrozenMessage : IPlayerActionLogMessage
    {
        public Guid RainbowBattleId { get; set; }
        public RainbowBattlePlayerDump Killer { get; set; }
        public RainbowBattlePlayerDump Killed { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}