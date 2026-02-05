// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsAPI.Data.Bazaar;

[ProtoContract]
public class BazaarItemDTO : ILongDto
{
    [ProtoMember(2)]
    public long CharacterId { get; set; }

    [ProtoMember(3)]
    public ItemInstanceDTO ItemInstance { get; set; }

    [ProtoMember(4)]
    public int Amount { get; set; }

    [ProtoMember(5)]
    public int SoldAmount { get; set; }

    [ProtoMember(6)]
    public bool IsPackage { get; set; }

    [ProtoMember(7)]
    public bool UsedMedal { get; set; }

    [ProtoMember(8)]
    public long PricePerItem { get; set; }

    [ProtoMember(9)]
    public long SaleFee { get; set; }

    [ProtoMember(10)]
    public DateTime ExpiryDate { get; set; }

    [ProtoMember(11)]
    public short DayExpiryAmount { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ProtoMember(1)]
    public long Id { get; set; }
}