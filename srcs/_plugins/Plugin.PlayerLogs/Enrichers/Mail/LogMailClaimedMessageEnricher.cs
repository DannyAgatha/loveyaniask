using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Mail;
using WingsEmu.Game.Mails.Events;

namespace Plugin.PlayerLogs.Enrichers.Mail
{
    public class LogMailClaimedMessageEnricher : ILogMessageEnricher<MailClaimedEvent, LogMailClaimedMessage>
    {
        public void Enrich(LogMailClaimedMessage message, MailClaimedEvent e)
        {
            message.MailId = e.MailId;
            message.SenderName = e.SenderName;
            message.ItemInstance = e.ItemInstance;
        }
    }
}