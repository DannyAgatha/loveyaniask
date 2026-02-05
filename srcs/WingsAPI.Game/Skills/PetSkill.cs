using System;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Managers.StaticData;

namespace WingsEmu.Game.Skills;

public class PetSkill : PetSkillDTO, IBattleEntitySkill
{
    private SkillDTO _skill;

    public DateTime LastUse { get; set; } = DateTime.MinValue;
    public short Rate => 100;

    public SkillDTO Skill => _skill ??= StaticSkillsManager.Instance.GetSkill(SkillId);
}