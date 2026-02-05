using System.Collections.Generic;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseSkillMartialArtist
{
    public List<CharacterSkillDTO> DefaultSkills { get; set; } =
    [
        new() { SkillVNum = 1525 }, new() { SkillVNum = 1526 }, new() { SkillVNum = 1527 },
        new() { SkillVNum = 1528 }, new() { SkillVNum = 1529 }, new() { SkillVNum = 1530 },
        new() { SkillVNum = 1531 }, new() { SkillVNum = 1532 }, new() { SkillVNum = 1533 },
        new() { SkillVNum = 1534 }, new() { SkillVNum = 1535 }, new() { SkillVNum = 1536 },
        new() { SkillVNum = 1537 }, new() { SkillVNum = 1538 }, new() { SkillVNum = 1539 }
    ];

    public List<CharacterSkillDTO> PassiveSkills { get; set; } =
    [
        new() { SkillVNum = 21 }, new() { SkillVNum = 25 }, new() { SkillVNum = 29 },
        new() { SkillVNum = 37 }, new() { SkillVNum = 41 }, new() { SkillVNum = 45 },
        new() { SkillVNum = 49 }, new() { SkillVNum = 53 }, new() { SkillVNum = 57 }
    ];
}