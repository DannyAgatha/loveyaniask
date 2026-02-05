using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Mail;
using WingsEmu.Game.Mails.Events;

namespace Plugin.PlayerLogs.Enrichers.Mail
{
    public class LogMailRemovedMessageEnricher : ILogMessageEnricher<MailRemovedEvent, LogMailRemovedMessage>
    {
        public void Enrich(LogMailRemovedMessage message, MailRemovedEvent e)
        {
            message.ItemInstance = e.ItemInstance;
            message.MailId = e.MailId;
            message.SenderName = e.SenderName;
        }
    }
}