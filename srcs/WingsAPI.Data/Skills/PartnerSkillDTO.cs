using ProtoBuf;

namespace WingsEmu.DTOs.Skills;

[ProtoContract]
public class PartnerSkillDTO
{
    [ProtoMember(1)]
    public int SkillId { get; set; }

    [ProtoMember(2)]
    public int Rank { get; set; }

    [ProtoMember(3)]
    public byte Slot { get; set; }
}