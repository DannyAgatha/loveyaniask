using System.Collections.Generic;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseSkillSwordman
{
    public List<CharacterSkillDTO> DefaultSkills { get; set; } =
    [
        new() { SkillVNum = 220 }, new() { SkillVNum = 221 }, new() { SkillVNum = 222 },
        new() { SkillVNum = 223 }, new() { SkillVNum = 224 }, new() { SkillVNum = 225 },
        new() { SkillVNum = 226 }, new() { SkillVNum = 227 }, new() { SkillVNum = 228 },
        new() { SkillVNum = 229 }, new() { SkillVNum = 230 }, new() { SkillVNum = 231 },
        new() { SkillVNum = 232 }, new() { SkillVNum = 233 }, new() { SkillVNum = 234 },
        new() { SkillVNum = 238 }
    ];

    public List<CharacterSkillDTO> PassiveSkills { get; set; } =
    [
        new() { SkillVNum = 21 }, new() { SkillVNum = 25 }, new() { SkillVNum = 29 },
        new() { SkillVNum = 37 }, new() { SkillVNum = 41 }, new() { SkillVNum = 45 },
        new() { SkillVNum = 49 }, new() { SkillVNum = 53 }, new() { SkillVNum = 57 }
    ];
}