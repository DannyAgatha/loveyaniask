using System;
using System.Collections.Generic;
using WingsEmu.Core.Generics;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Entities;

public interface INpcMonsterEntity : IAiEntity, IMonsterData
{
    SkillInfo BasicSkill { get; }
    ThreadSafeHashSet<IBattleEntity> Damagers { get; }
    ThreadSafeHashSet<IBattleEntity> Targets { get; }
    ThreadSafeHashSet<(VisualType, long)> TargetsByVisualTypeAndId { get; }
    DateTime LastTargetsRefresh { get; set; }
    DateTime Death { get; set; }
    short FirstX { get; set; }
    short FirstY { get; set; }
    DateTime LastEffect { get; set; }
    DateTime LastSkill { get; set; }
    DateTime LastSpecialHpDecrease { get; set; }
    bool ShouldRespawn { get; }
    bool ReturningToFirstPosition { get; set; }
    bool ShouldFindNewTarget { get; set; }
    bool FindNewPositionAroundTarget { get; set; }
    bool IsApproachingTarget { get; set; }
    bool OnFirstDamageReceive { get; set; }
    DateTime SpawnDate { get; set; }
    IBattleEntity Target { get; set; }
    DateTime NextTick { get; set; }
    DateTime NextAttackReady { get; set; }
    bool ModeIsActive { get; set; }
    short Morph { get; set; }
    long ModeDeathsSinceRespawn { get; set; }
    (VisualType, long) LastAttackedEntity { get; set; }
    bool IsRunningAway { get; set; }
    byte ReturnTimeOut { get; set; }

    IReadOnlyList<INpcMonsterSkill> NotBasicSkills { get; }
    IReadOnlyList<INpcMonsterSkill> SkillsWithoutDashSkill { get; }
    INpcMonsterSkill ReplacedBasicSkill { get; }
    INpcMonsterSkill DashSkill { get; }
}