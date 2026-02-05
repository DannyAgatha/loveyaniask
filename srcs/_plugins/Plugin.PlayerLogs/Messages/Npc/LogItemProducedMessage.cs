using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Npc
{
    [MessageType("logs.npc.item-produced")]
    public class LogItemProducedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int ItemAmount { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}