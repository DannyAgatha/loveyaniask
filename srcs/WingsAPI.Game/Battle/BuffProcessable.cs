using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class BuffProcessable
{
    public BuffProcessable(IBattleEntity caster, IBattleEntity target, SkillCast skillCast, Position position = default)
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
}