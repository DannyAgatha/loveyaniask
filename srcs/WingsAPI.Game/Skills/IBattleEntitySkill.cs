using System;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Game.Skills;

public interface IBattleEntitySkill
{
    SkillDTO Skill { get; }
    DateTime LastUse { get; set; }
    short Rate { get; }
}