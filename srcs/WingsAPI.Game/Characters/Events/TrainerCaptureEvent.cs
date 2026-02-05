using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Characters.Events;

public class TrainerCaptureEvent : PlayerEvent
{
    public TrainerCaptureEvent(IMonsterEntity target, bool isSkill, int captureRate, SkillInfo skill = null)
    {
        Target = target;
        IsSkill = isSkill;
        Skill = skill;
        CaptureRate = captureRate;
    }

    public IMonsterEntity Target { get; }
    public bool IsSkill { get; }
    public int CaptureRate { get; }
    public SkillInfo Skill { get; }
}