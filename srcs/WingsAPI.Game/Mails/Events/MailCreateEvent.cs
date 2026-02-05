using WingsEmu.DTOs.Mails;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Mails.Events;

public class MailCreateEvent : PlayerEvent
{
    public MailCreateEvent(string senderName, long receiverId, MailGiftType mailGiftType, GameItemInstance itemInstance)
    {
        SenderName = senderName;
        ReceiverId = receiverId;
        MailGiftType = mailGiftType;
        ItemInstance = itemInstance;
    }

    public string SenderName { get; }

    public long ReceiverId { get; }

    public MailGiftType MailGiftType { get; }

    public GameItemInstance ItemInstance { get; }
}