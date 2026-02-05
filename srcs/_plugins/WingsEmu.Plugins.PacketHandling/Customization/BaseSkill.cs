// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Customization.NewCharCustomisation;

public class BaseSkill
{
    public BaseSkill() => Skills = new List<CharacterSkillDTO>
    {
        new() { SkillVNum = 200 },
        new() { SkillVNum = 201 },
        new() { SkillVNum = 209 }
    };

    public List<CharacterSkillDTO> Skills { get; set; }
}