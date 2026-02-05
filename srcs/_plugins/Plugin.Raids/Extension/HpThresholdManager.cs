using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Extension;

public class HpThresholdManager
{
    private readonly List<MonsterHpThresholdConfig> _configs;
    public readonly IAsyncEventPipeline _asyncEventPipeline;

    public HpThresholdManager(List<MonsterHpThresholdConfig> configs, IAsyncEventPipeline asyncEventPipeline)
    {
        _configs = configs;
        _asyncEventPipeline = asyncEventPipeline;
    }

    /// <summary>
    /// Process the HP thresholds for a specific defender.
    /// </summary>
    /// <param name="defender"></param>
    /// <param name="initialDefenderHp"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task ProcessHpThresholdsAsync(IBattleEntity defender, float initialDefenderHp, CancellationToken cancellation)
    {
        if (defender is not IMonsterEntity monsterEntity)
            return;

        IEnumerable<MonsterHpThresholdConfig> configsForMonster = _configs.Where(c => (int)c.MonsterVNum == monsterEntity.MonsterVNum);

        foreach (MonsterHpThresholdConfig config in configsForMonster)
        {
            foreach (HpThresholdConfig threshold in config.HpThresholds.OrderByDescending(t => t.HpPercentage))
            {
                float hpThreshold = monsterEntity.FakeMaxHp.GetValueOrDefault() * threshold.HpPercentage;

                if (defender.Hp <= hpThreshold && hpThreshold <= initialDefenderHp)
                {
                    if (threshold.SkillConditions != null)
                    {
                        await ProcessSkillConditionsAsync(defender, threshold.SkillConditions, cancellation);
                    }
                    await defender.TriggerEvents(threshold.Trigger);
                }
            }
        }
    }

    /// <summary>
    /// Process using a specific skill depending on conditions for a specific defender.
    /// </summary>
    /// <param name="defender"></param>
    /// <param name="skillConditions"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    private async Task ProcessSkillConditionsAsync(IBattleEntity defender, List<SkillMonsterCondition> skillConditions, CancellationToken cancellation)
    {
        IEnumerable<SkillMonsterCondition> validConditions = skillConditions
            .Where(condition => defender is IMonsterEntity monsterEntity && monsterEntity.MonsterVNum == (short)condition.MonsterVNum);

        foreach (SkillMonsterCondition condition in validConditions)
        {
            if (defender is IMonsterEntity monsterEntity)
            {
                monsterEntity.SkillToUse = (short)condition.SkillVNum;
                monsterEntity.ForceUseSkill = true;
            }
            await _asyncEventPipeline.ProcessEventAsync(new RaidProcessBossMechanicsEvent
            {
                BattleEntity = defender,
                SkillInfo = new SkillInfo { Vnum = (int)condition.SkillVNum }
            }, cancellation);
        }
    }
}

public class MonsterHpThresholdConfig
{
    public int MonsterVNum { get; init; }
    public List<HpThresholdConfig> HpThresholds { get; init; } = new();
}

public class HpThresholdConfig
{
    public float HpPercentage { get; init; }
    public List<SkillMonsterCondition> SkillConditions { get; init; } = new();
    public string Trigger { get; init; }
}
