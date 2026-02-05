using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class MailOpenEvent : PlayerEvent
{
    public MailOpenEvent(long mailId) => MailId = mailId;

    public long MailId { get; }
}