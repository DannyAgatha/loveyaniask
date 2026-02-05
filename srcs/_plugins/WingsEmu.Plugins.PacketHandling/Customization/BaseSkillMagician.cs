using System.Collections.Generic;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseSkillMagician
{
    public List<CharacterSkillDTO> DefaultSkills { get; set; } =
    [
        new() { SkillVNum = 260 }, new() { SkillVNum = 261 }, new() { SkillVNum = 262 },
        new() { SkillVNum = 263 }, new() { SkillVNum = 264 }, new() { SkillVNum = 265 },
        new() { SkillVNum = 266 }, new() { SkillVNum = 267 }, new() { SkillVNum = 268 },
        new() { SkillVNum = 269 }, new() { SkillVNum = 270 }, new() { SkillVNum = 271 },
        new() { SkillVNum = 272 }, new() { SkillVNum = 273 }, new() { SkillVNum = 274 },
        new() { SkillVNum = 275 }, new() { SkillVNum = 276 }, new() { SkillVNum = 277 }
    ];

    public List<CharacterSkillDTO> PassiveSkills { get; set; } =
    [
        new() { SkillVNum = 21 }, new() { SkillVNum = 25 }, new() { SkillVNum = 29 },
        new() { SkillVNum = 37 }, new() { SkillVNum = 41 }, new() { SkillVNum = 45 },
        new() { SkillVNum = 49 }, new() { SkillVNum = 53 }, new() { SkillVNum = 57 }
    ];
}