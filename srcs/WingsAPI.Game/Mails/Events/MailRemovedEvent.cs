using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class MailRemovedEvent : PlayerEvent
{
    public long MailId { get; init; }
    public string SenderName { get; init; }
    public ItemInstanceDTO ItemInstance { get; init; }
}