using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgrade.sp-upgraded")]
    public class LogSpUpgradedMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO Sp { get; set; }
        public string Mode { get; set; }
        public string Result { get; set; }
        public short OriginalUpgrade { get; set; }
        public bool IsProtected { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}