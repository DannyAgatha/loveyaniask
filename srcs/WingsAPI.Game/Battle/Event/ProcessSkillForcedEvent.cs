using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Battle.Event;

public class ProcessSkillForcedEvent : IAsyncEvent
{
    public ProcessSkillForcedEvent(float hpThresholdPercentage, List<SkillMonsterCondition> conditions)
    {
        HpThresholdPercentage = hpThresholdPercentage;
        Conditions = conditions;
    }

    public float HpThresholdPercentage { get; }
    public List<SkillMonsterCondition> Conditions { get; }
}

public class SkillMonsterCondition
{
    public SkillsVnums SkillVNum { get; init; }
    public MonsterVnum MonsterVNum { get; init; }
}