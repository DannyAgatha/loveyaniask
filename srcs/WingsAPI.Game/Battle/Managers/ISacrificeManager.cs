using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Battle;

public interface ISacrificeManager
{
    void SaveSacrifice(IBattleEntity caster, IBattleEntity target);
    IBattleEntity GetTarget(IBattleEntity caster);
    IBattleEntity GetCaster(IBattleEntity target);
    void RemoveSacrifice(IBattleEntity caster, IBattleEntity target);
}