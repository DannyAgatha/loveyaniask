using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Helpers;

namespace Plugin.PlayerLogs.Messages.LevelUp
{
    [MessageType("logs.levelup.nosmate")]
    public class LogLevelUpNosMateMessage : IPlayerActionLogMessage
    {
        public byte Level { get; set; }
        public int NosMateMonsterVnum { get; set; }
        public string LevelUpType { get; set; }
        public int? ItemVnum { get; set; }
        public Location Location { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}