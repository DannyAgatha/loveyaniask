// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsEmu.DTOs.Mails;

[ProtoContract]
public class CharacterMailDto : ILongDto
{
    [ProtoMember(2)]
    public DateTime Date { get; set; }

    [ProtoMember(3)]
    public string SenderName { get; set; }

    [ProtoMember(4)]
    public long ReceiverId { get; set; }

    [ProtoMember(5)]
    public MailGiftType MailGiftType { get; set; }

    [ProtoMember(6)]
    public ItemInstanceDTO ItemInstance { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ProtoMember(1)]
    public long Id { get; set; }
}