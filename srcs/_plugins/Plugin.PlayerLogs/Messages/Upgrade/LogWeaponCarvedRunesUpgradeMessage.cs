using PhoenixLib.ServiceBus.Routing;
using System;
using WingsEmu.DTOs.Items;

namespace Plugin.PlayerLogs.Messages.Upgrade
{
    [MessageType("logs.upgrade.weapon-carved-runes-upgraded")]
    public class LogWeaponCarvedRunesUpgradeMessage : IPlayerActionLogMessage
    {
        public ItemInstanceDTO Weapon { get; set; }
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