using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Act4
{
    [MessageType("logs.act4.dungeon-started")]
    public class LogAct4DungeonStartedMessage : IPlayerActionLogMessage
    {
        public string FactionType { get; set; }
        public string DungeonType { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}