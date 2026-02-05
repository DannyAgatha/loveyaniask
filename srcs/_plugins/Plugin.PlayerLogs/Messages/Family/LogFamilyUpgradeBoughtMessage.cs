using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Family
{
    [MessageType("logs.family.upgrade-bought")]
    public class LogFamilyUpgradeBoughtMessage : IPlayerActionLogMessage
    {
        public long FamilyId { get; set; }
        public int UpgradeVnum { get; set; }
        public string FamilyUpgradeType { get; set; }
        public short UpgradeValue { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}