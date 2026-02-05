using System;
using System.Collections.Generic;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Player
{
    [MessageType("logs.box.opened")]
    public class LogBoxOpenedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO Box { get; set; }
        public List<ItemInstanceDTO> Rewards { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}