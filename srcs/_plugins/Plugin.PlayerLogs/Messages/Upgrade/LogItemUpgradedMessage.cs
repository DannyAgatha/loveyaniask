using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgraded.itemupgraded")]
    public class LogItemUpgradedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO Item { get; set; }
        public long TotalPrice { get; set; }
        public string Mode { get; set; }
        public string Protection { get; set; }
        public bool HasAmulet { get; set; }
        public short OriginalUpgrade { get; set; }
        public string Result { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}