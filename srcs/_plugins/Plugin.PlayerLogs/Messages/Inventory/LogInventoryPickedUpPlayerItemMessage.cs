using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Helpers;

namespace Plugin.PlayerLogs.Messages.Inventory
{
    [MessageType("logs.inventory.picked-up-player-item")]
    public class LogInventoryPickedUpPlayerItemMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO ItemInstance { get; set; }
        public int Amount { get; set; }
        public Location Location { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}