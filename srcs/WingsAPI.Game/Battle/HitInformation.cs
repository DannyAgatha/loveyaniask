using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class HitInformation
{
    public HitInformation(IBattleEntity caster, SkillInfo skill, Position position) : this(caster, skill) => Position = position;

    public HitInformation(IBattleEntity caster, SkillInfo skill)
    {
        Caster = caster;
        Skill = skill;
        IsFirst = true;
    }

    public IBattleEntity Caster { get; }
    public SkillInfo Skill { get; }
    public Position Position { get; }
    public bool IsFirst { get; set; }
}