using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgraded.cellon")]
    public class LogCellonUpgradedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO Item { get; set; }
        public int CellonVnum { get; set; }
        public bool Succeed { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}