// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace WingsAPI.Data.Account;

[ProtoContract]
public class AccountDTO : ILongDto
{
    [ProtoMember(1)]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [ProtoMember(2)]
    public Guid MasterAccountId { get; set; }

    [ProtoMember(3)]
    public AuthorityType Authority { get; set; }

    [ProtoMember(4)]
    public AccountLanguage Language { get; set; }

    [ProtoMember(5)]
    public long BankMoney { get; set; }

    [ProtoMember(6)]
    public bool IsPrimaryAccount { get; set; }

    [ProtoMember(7)]
    public string Name { get; set; }

    [ProtoMember(8)]
    public string Password { get; set; }

    [ProtoMember(20)]
    public string RegistrationIP { get; set; }

}