using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class MailRemoveEvent : PlayerEvent
{
    public MailRemoveEvent(long mailId) => MailId = mailId;

    public long MailId { get; }
}