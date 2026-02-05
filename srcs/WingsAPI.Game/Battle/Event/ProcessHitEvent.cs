// NosEmu
// 


using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class ProcessHitEvent : IAsyncEvent
{
    public ProcessHitEvent(IBattleEntity caster, IBattleEntity target, SkillInfo skill, Position position)
    {
        Caster = caster;
        Target = target;
        HitInformation = new HitInformation(caster, skill, position);
    }

    public IBattleEntity Caster { get; }
    public IBattleEntity Target { get; }
    public HitInformation HitInformation { get; }
}