// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyMembershipDto : ILongDto
{
    [ProtoMember(2)]
    public long FamilyId { get; set; }

    [ProtoMember(3)]
    public long CharacterId { get; set; }

    [ProtoMember(4)]
    public FamilyAuthority Authority { get; set; }

    [ProtoMember(5)]
    [MaxLength(50)]
    public string DailyMessage { get; set; }

    [ProtoMember(6)]
    public long Experience { get; set; }

    [ProtoMember(7)]
    public FamilyTitle Title { get; set; }

    [ProtoMember(8)]
    public DateTime JoinDate { get; set; }

    [ProtoMember(9)]
    public DateTime LastOnlineDate { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ProtoMember(1)]
    public long Id { get; set; }
}