using System.Collections.Generic;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseSkillArcher
{
    public List<CharacterSkillDTO> DefaultSkills { get; set; } =
    [
        new() { SkillVNum = 240 }, new() { SkillVNum = 241 }, new() { SkillVNum = 242 },
        new() { SkillVNum = 243 }, new() { SkillVNum = 244 }, new() { SkillVNum = 245 },
        new() { SkillVNum = 246 }, new() { SkillVNum = 247 }, new() { SkillVNum = 248 },
        new() { SkillVNum = 249 }, new() { SkillVNum = 250 }, new() { SkillVNum = 251 },
        new() { SkillVNum = 252 }, new() { SkillVNum = 253 }, new() { SkillVNum = 254 },
        new() { SkillVNum = 255 }, new() { SkillVNum = 256 }
    ];

    public List<CharacterSkillDTO> PassiveSkills { get; set; } =
    [
        new() { SkillVNum = 21 }, new() { SkillVNum = 25 }, new() { SkillVNum = 29 },
        new() { SkillVNum = 37 }, new() { SkillVNum = 41 }, new() { SkillVNum = 45 },
        new() { SkillVNum = 49 }, new() { SkillVNum = 53 }, new() { SkillVNum = 57 }
    ];
}