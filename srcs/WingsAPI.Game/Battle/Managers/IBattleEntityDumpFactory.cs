using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.Battle;

public interface IBattleEntityDumpFactory
{
    IBattleEntityDump Dump(IPlayerEntity entity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false);
    IBattleEntityDump Dump(IMonsterEntity entity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false);
    IBattleEntityDump Dump(INpcEntity npcEntity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false);
    IBattleEntityDump Dump(IMateEntity entity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false);
}