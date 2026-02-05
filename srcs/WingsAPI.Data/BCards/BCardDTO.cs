// NosEmu
// 
// Developed by NosWings Team

using System;
using ProtoBuf;
using WingsEmu.Game._enum;

namespace WingsEmu.DTOs.BCards;

[ProtoContract]
public class BCardDTO
{
    [ProtoMember(1)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ProtoMember(2)]
    public byte SubType { get; set; }

    [ProtoMember(3)]
    public int Type { get; set; }

    [ProtoMember(4)]
    public int FirstData { get; set; }

    [ProtoMember(5)]
    public int SecondData { get; set; }

    [ProtoMember(6)]
    public int ProcChance { get; set; }

    [ProtoMember(7)]
    public double? TickPeriod { get; set; }

    [ProtoMember(8)]
    public byte CastType { get; set; }

    [ProtoMember(9)]
    public BCardScalingType FirstDataScalingType { get; set; }

    [ProtoMember(10)]
    public BCardScalingType SecondDataScalingType { get; set; }

    [ProtoMember(11)]
    public bool? IsSecondBCardExecution { get; set; }

    [ProtoMember(12)]
    public int? CardId { get; set; }

    [ProtoMember(13)]
    public int? ItemVNum { get; set; }

    [ProtoMember(14)]
    public int? SkillVNum { get; set; }

    [ProtoMember(15)]
    public int? NpcMonsterVNum { get; set; }

    [ProtoMember(16)]
    public BCardNpcMonsterTriggerType? TriggerType { get; set; }

    [ProtoMember(17)]
    public BCardNpcTriggerType? NpcTriggerType { get; set; }

    [ProtoMember(18)]
    public bool IsMonsterMode { get; set; }
    
    [ProtoMember(19)]
    public bool IsRunePower { get; set; }
    
    [ProtoMember(20)]
    public byte BCardLevel { get; set; }
}