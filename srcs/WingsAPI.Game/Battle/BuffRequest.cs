using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class BuffRequest
{
    public BuffRequest(IBattleEntity caster, IEnumerable<IBattleEntity> targets, SkillCast skillCast, Position position, IBattleEntity target = null)
    {
        Caster = caster;
        Targets = targets;
        SkillCast = skillCast;
        Position = position;
        Target = target;
    }

    public IBattleEntity Caster { get; }
    public IBattleEntity Target { get; }
    public IEnumerable<IBattleEntity> Targets { get; }
    public SkillCast SkillCast { get; }
    public Position Position { get; }
}