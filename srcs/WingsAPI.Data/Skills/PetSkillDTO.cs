using ProtoBuf;

namespace WingsEmu.DTOs.Skills;

[ProtoContract]
public class PetSkillDTO
{
    [ProtoMember(1)]
    public int SkillId { get; set; }
}