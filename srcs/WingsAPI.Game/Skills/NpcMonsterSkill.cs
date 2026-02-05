// NosEmu
// 


using System;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Game.Skills;

public interface INpcMonsterSkill : IBattleEntitySkill
{
    bool IsBasicAttack { get; }
    bool IsIgnoringHitChance { get; }
}

public class NpcMonsterSkill : INpcMonsterSkill
{
    public NpcMonsterSkill(SkillDTO skill, short rate, bool isBasicAttack, bool isIgnoringHitChance)
    {
        Skill = skill;
        Rate = rate;
        IsBasicAttack = isBasicAttack;
        IsIgnoringHitChance = isIgnoringHitChance;
    }

    public short Rate { get; }
    public bool IsIgnoringHitChance { get; }
    public bool IsBasicAttack { get; }

    public DateTime LastUse { get; set; } = DateTime.MinValue;
    public SkillDTO Skill { get; }
}