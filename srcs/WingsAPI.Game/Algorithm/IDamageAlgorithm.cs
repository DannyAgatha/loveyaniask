using WingsEmu.Game.Battle;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Algorithm;

public interface IDamageAlgorithm
{
    DamageAlgorithmResult GenerateDamage(IBattleEntityDump attacker, IBattleEntityDump target, SkillInfo skill);
}