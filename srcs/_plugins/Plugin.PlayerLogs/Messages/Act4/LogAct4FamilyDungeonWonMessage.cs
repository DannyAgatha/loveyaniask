using System;
using System.Collections.Generic;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Act4;

namespace Plugin.PlayerLogs.Messages.Act4
{
    [MessageType("log.act4.dungeon-won")]
    public class LogAct4FamilyDungeonWonMessage : IPlayerActionLogMessage
    {
        public long FamilyId { get; set; }
        public DungeonType DungeonType { get; set; }
        public IEnumerable<int> DungeonMembers { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}