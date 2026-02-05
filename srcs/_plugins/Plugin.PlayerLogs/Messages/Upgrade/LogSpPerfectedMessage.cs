using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgrade.sp-perfected")]
    public class LogSpPerfectedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO Sp { get; set; }
        public bool Success { get; set; }
        public byte SpPerfectionLevel { get; set; }

        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}