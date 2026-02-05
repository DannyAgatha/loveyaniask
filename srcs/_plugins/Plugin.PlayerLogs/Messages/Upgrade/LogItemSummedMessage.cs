using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgraded.itemsummed")]
    public class LogItemSummedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO LeftItem { get; set; }
        public ItemInstanceDTO RightItem { get; set; }
        public bool Succeed { get; set; }
        public int SumLevel { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}