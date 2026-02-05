using System;

namespace WingsEmu.Game.Battle;

public interface ICastingComponent
{
    public SkillCast SkillCast { get; }
    public bool IsCastingSkill { get; }
    public void SetCastingSkill(SkillInfo skill, DateTime time);
    public void RemoveCastingSkill();
}

public class CastingComponent : ICastingComponent
{
    public SkillCast SkillCast { get; private set; }

    public bool IsCastingSkill => SkillCast != null;

    public void SetCastingSkill(SkillInfo skill, DateTime time)
    {
        if (SkillCast != null)
        {
            return;
        }

        var skillCast = new SkillCast(skill, time);
        SkillCast = skillCast;
    }

    public void RemoveCastingSkill()
    {
        if (SkillCast == null)
        {
            return;
        }

        SkillCast = null;
    }
}

public class SkillCast
{
    public SkillCast(SkillInfo skill, DateTime skillEndCastTime)
    {
        Skill = skill;
        SkillEndCastTime = skillEndCastTime;
    }

    public SkillInfo Skill { get; }

    public DateTime SkillEndCastTime { get; set; }
}