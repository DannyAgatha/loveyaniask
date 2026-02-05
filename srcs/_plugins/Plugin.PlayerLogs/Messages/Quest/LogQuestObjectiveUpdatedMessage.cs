using System;
using System.Collections.Generic;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Quests;

namespace Plugin.PlayerLogs.Messages.Quest
{
    [MessageType("logs.quest.objectiveupdated")]
    public class LogQuestObjectiveUpdatedMessage : IPlayerActionLogMessage
    {
        public int QuestId { get; set; }
        public string SlotType { get; set; }
        public Dictionary<int, CharacterQuestObjectiveDto> UpdatedObjectivesAmount { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}