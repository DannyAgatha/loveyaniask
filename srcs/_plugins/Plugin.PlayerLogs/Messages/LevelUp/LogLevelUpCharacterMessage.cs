using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Helpers;

namespace Plugin.PlayerLogs.Messages.LevelUp
{
    [MessageType("logs.levelup.character")]
    public class LogLevelUpCharacterMessage : IPlayerActionLogMessage
    {
        public int Level { get; set; }
        public string LevelType { get; set; }
        public Location Location { get; set; }
        public int? ItemVnum { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}