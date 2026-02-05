using System;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class BattleExecuteSkillEvent : IBattleEntityEvent
{
    public BattleExecuteSkillEvent(IBattleEntity entity, IBattleEntity target, SkillInfo skillInfo, DateTime endSkillCastTime, Position position = default)
    {
        Entity = entity;
        Target = target;
        SkillInfo = skillInfo;
        Position = position;
        EndSkillCastTime = endSkillCastTime;
    }

    public IBattleEntity Target { get; }
    public SkillInfo SkillInfo { get; }
    public DateTime EndSkillCastTime { get; }
    public Position Position { get; }
    public IBattleEntity Entity { get; }
}