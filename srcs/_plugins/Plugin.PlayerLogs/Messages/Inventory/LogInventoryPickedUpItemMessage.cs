using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Helpers;

namespace Plugin.PlayerLogs.Messages.Inventory
{
    [MessageType("logs.inventory.pickedupitem")]
    public class LogInventoryPickedUpItemMessage : IPlayerActionLogMessage
    {
        public int ItemVnum { get; set; }
        public int Amount { get; set; }
        public Location Location { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}