using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class HitProcessable
{
    public HitProcessable(IBattleEntity caster, IBattleEntity target, SkillCast skillCast, Position position)
    {
        Caster = caster;
        Target = target;
        SkillCast = skillCast;
        Position = position;
    }

    public IBattleEntity Caster { get; }
    public IBattleEntity Target { get; }
    public SkillCast SkillCast { get; }
    public Position Position { get; }
    public bool IsJump { get; set; }
}