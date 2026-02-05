// NosEmu
// 


using System;
using ProtoBuf;

namespace WingsEmu.DTOs.Bonus;

[ProtoContract]
public class CharacterStaticBonusDto
{
    [ProtoMember(1)]
    public DateTime? DateEnd { get; set; }

    [ProtoMember(2)]
    public StaticBonusType StaticBonusType { get; set; }

    [ProtoMember(3)]
    public int ItemVnum { get; set; }
}