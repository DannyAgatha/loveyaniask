using System;
using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game._ECS.Systems;

public interface IMonsterSystem
{
    IMonsterEntity GetMonsterById(long id);
    IMonsterEntity GetMonsterByUniqueId(Guid id);
    IReadOnlyList<IMonsterEntity> GetAliveMonsters();
    IReadOnlyList<IMonsterEntity> GetDeadMonsters();
    IReadOnlyList<IMonsterEntity> GetAliveMonsters(Func<IMonsterEntity, bool> predicate);
    List<IMonsterEntity> GetMonstersAlive();
    List<IMonsterEntity> GetRemainingTotems();
    IReadOnlyList<IMonsterEntity> GetAliveMonstersInRange(Position pos, short distance);
    IReadOnlyList<IMonsterEntity> GetClosestMonstersInRange(Position pos, short distance);
    void AddMonster(IMonsterEntity entity);
    void RemoveMonster(IMonsterEntity entity);

    /// <summary>
    ///     This method will be used only if player/mate used some kind of debuff or voke to provoke monster, but it won't be
    ///     added to Monster's Damagers list.
    /// </summary>
    /// <param name="monsterEntity"></param>
    /// <param name="target"></param>
    /// <param name="time"></param>
    void AddEntityToTargets(IMonsterEntity monsterEntity, IBattleEntity target);

    void MonsterRefreshTarget(IMonsterEntity target, IBattleEntity caster, in DateTime time, bool isByAttacking = false);
    void ForgetAll(IMonsterEntity monsterEntity, in DateTime time, bool clearDamagers = true);
    void RemoveTarget(IMonsterEntity monsterEntity, IBattleEntity entity, bool checkIfPlayer = false);

    void ActivateMode(IMonsterEntity monsterEntity);
    void DeactivateMode(IMonsterEntity monsterEntity);

    void IncreaseMonsterDeathsOnMap();
    long MonsterDeathsOnMap();
    byte CurrentVessels();
    bool IsSummonLimitReached(int? summonerId, SummonType? summonSummonType);
}