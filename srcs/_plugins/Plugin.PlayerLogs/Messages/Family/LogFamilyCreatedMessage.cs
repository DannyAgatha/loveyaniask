using System;
using System.Collections.Generic;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.PlayerLogs.Messages.Family
{
    [MessageType("logs.family.created")]
    public class LogFamilyCreatedMessage : IPlayerActionLogMessage
    {
        public long FamilyId { get; set; }
        public string FamilyName { get; set; }
        public List<long> DeputiesIds { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public int Level { get; set; }
        public int HeroLevel { get; set; }
        public ClassType Class { get; set; }
        public string IpAddress { get; init; }
    }
}