using PhoenixLib.ServiceBus.Routing;
using System;
using WingsEmu.Game.Helpers;

namespace Plugin.PlayerLogs.Messages.HeroicLevelUp
{
    [MessageType("logs.heroiclevelup.nosmate")]
    public class LogHeroicLevelUpNosMateMessage : IPlayerActionLogMessage
    {
        public byte HeroLevel { get; set; }
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