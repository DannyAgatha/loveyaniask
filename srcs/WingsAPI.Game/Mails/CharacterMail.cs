using WingsEmu.DTOs.Mails;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Mails;

public class CharacterMail : CharacterMailDto
{
    public CharacterMail(CharacterMailDto mailDto, byte mailSlot, GameItemInstance itemInstance)
    {
        MailSlot = mailSlot;
        ItemInstance = itemInstance;
        Id = mailDto.Id;
        Date = mailDto.Date;
        SenderName = mailDto.SenderName;
        ReceiverId = mailDto.ReceiverId;
        MailGiftType = mailDto.MailGiftType;
    }

    public byte MailSlot { get; }

    public GameItemInstance ItemInstance { get; }
}