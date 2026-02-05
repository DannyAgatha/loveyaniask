using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Mail;
using WingsEmu.Game.Mails.Events;

namespace Plugin.PlayerLogs.Enrichers.Mail
{
    public class LogNoteSentMessageEnricher : ILogMessageEnricher<NoteSentEvent, LogNoteSentMessage>
    {
        public void Enrich(LogNoteSentMessage message, NoteSentEvent e)
        {
            message.Message = e.Message;
            message.NoteId = e.NoteId;
            message.ReceiverName = e.ReceiverName;
            message.Title = e.Title;
        }
    }
}