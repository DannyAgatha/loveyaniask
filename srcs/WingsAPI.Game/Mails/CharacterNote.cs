using WingsEmu.DTOs.Mails;

namespace WingsEmu.Game.Mails;

public class CharacterNote : CharacterNoteDto
{
    public CharacterNote(CharacterNoteDto noteDto, byte noteSlot)
    {
        NoteSlot = noteSlot;

        Id = noteDto.Id;
        Date = noteDto.Date;
        SenderId = noteDto.SenderId;
        ReceiverId = noteDto.ReceiverId;
        Title = noteDto.Title;
        Message = noteDto.Message;
        EquipmentPackets = noteDto.EquipmentPackets;
        IsSenderCopy = noteDto.IsSenderCopy;
        IsOpened = noteDto.IsOpened;
        SenderGender = noteDto.SenderGender;
        SenderClass = noteDto.SenderClass;
        SenderHairColor = noteDto.SenderHairColor;
        SenderHairStyle = noteDto.SenderHairStyle;
        SenderName = noteDto.SenderName;
        ReceiverName = noteDto.ReceiverName;
    }

    public byte NoteSlot { get; }
}