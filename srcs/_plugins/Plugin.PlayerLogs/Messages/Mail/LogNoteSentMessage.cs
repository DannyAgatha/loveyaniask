using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Mail
{
    [MessageType("logs.note.sent")]
    public class LogNoteSentMessage : IPlayerActionLogMessage
    {
        public long NoteId { get; set; }
        public string ReceiverName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}