// NosEmu
// 


using ProtoBuf;

namespace WingsEmu.DTOs.Items;

[ProtoContract]
public class EquipmentOptionDTO
{
    [ProtoMember(1)]
    public EquipmentOptionType EquipmentOptionType { get; set; }

    [ProtoMember(2)]
    public int EffectVnum { get; set; }

    [ProtoMember(3)]
    public int Level { get; set; }

    [ProtoMember(4)]
    public byte Type { get; set; }

    [ProtoMember(5)]
    public int Value { get; set; }

    [ProtoMember(6)]
    public byte Weight { get; set; }

    [ProtoMember(7)]
    public bool ShellEffectUpgrade { get; set; }
}