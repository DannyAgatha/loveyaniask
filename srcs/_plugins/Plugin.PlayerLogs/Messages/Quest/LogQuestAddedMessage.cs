using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Quest
{
    [MessageType("logs.quest.added")]
    public class LogQuestAddedMessage : IPlayerActionLogMessage
    {
        public int QuestId { get; set; }
        public string SlotType { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}