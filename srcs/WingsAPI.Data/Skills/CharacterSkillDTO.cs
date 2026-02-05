// NosEmu
// 


using ProtoBuf;

namespace WingsEmu.DTOs.Skills;

[ProtoContract]
public class CharacterSkillDTO
{
    [ProtoMember(1)]
    public int SkillVNum { get; set; }

    [ProtoMember(2)]
    public int UpgradeSkill { get; set; }
}