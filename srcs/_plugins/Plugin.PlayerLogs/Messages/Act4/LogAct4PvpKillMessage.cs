using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Act4
{
    [MessageType("logs.act4.pvp-kill")]
    public class LogAct4PvpKillMessage : IPlayerActionLogMessage
    {
        public long TargetId { get; set; }
        public string KillerFaction { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}