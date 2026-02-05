using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public interface ISkillExecutor
{
    void ExecuteDamageZoneHitSkill(IBattleEntity caster, SkillCast skill, Position position);
    void ExecuteBuffZoneHitSkill(IBattleEntity caster, SkillCast skill, Position position);
    void ExecuteDebuffZoneHitSkill(IBattleEntity caster, SkillCast skill, Position position);
    void ExecuteDamageSkill(IBattleEntity caster, IBattleEntity target, SkillCast skill, Position positionBeforeDash = default);
    void ExecuteBuffSkill(IBattleEntity caster, IBattleEntity target, SkillCast skill);
    void ExecuteDebuffSkill(IBattleEntity caster, IBattleEntity target, SkillCast skill);
}