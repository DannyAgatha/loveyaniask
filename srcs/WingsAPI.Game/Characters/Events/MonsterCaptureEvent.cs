using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Characters.Events;

public class MonsterCaptureEvent : PlayerEvent
{
    public MonsterCaptureEvent(IMonsterEntity target, bool isSkill, SkillInfo skill = null)
    {
        Target = target;
        IsSkill = isSkill;
        Skill = skill;
    }

    public IMonsterEntity Target { get; }
    public bool IsSkill { get; }
    public SkillInfo Skill { get; }
}