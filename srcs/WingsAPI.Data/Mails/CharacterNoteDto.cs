using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.DTOs.Mails;

[ProtoContract]
public class CharacterNoteDto : ILongDto
{
    [ProtoMember(2)]
    public DateTime Date { get; set; }

    [ProtoMember(3)]
    public long SenderId { get; set; }

    [ProtoMember(4)]
    public long ReceiverId { get; set; }

    [ProtoMember(5)]
    public string Title { get; set; }

    [ProtoMember(6)]
    public string Message { get; set; }

    [ProtoMember(7)]
    public string EquipmentPackets { get; set; }

    [ProtoMember(8)]
    public bool IsSenderCopy { get; set; }

    [ProtoMember(9)]
    public bool IsOpened { get; set; }

    [ProtoMember(10)]
    public GenderType SenderGender { get; set; }

    [ProtoMember(11)]
    public ClassType SenderClass { get; set; }

    [ProtoMember(12)]
    public HairColorType SenderHairColor { get; set; }

    [ProtoMember(13)]
    public HairStyleType SenderHairStyle { get; set; }

    [ProtoMember(14)]
    public string SenderName { get; set; }

    [ProtoMember(15)]
    public string ReceiverName { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ProtoMember(1)]
    public long Id { get; set; }
}