using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.Battle;

public class BattleEntityDumpFactory : IBattleEntityDumpFactory
{
    public IBattleEntityDump Dump(IPlayerEntity entity, SkillInfo skill, bool isDefender = false, bool isMainTarget = false)
        => new PlayerBattleEntityDump(entity, skill, isDefender, isMainTarget);

    public IBattleEntityDump Dump(IMonsterEntity entity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false)
        => new NpcMonsterEntityDump(entity, entity, skillCasted, isDefender, isMainTarget);

    public IBattleEntityDump Dump(INpcEntity npcEntity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false)
        => new NpcMonsterEntityDump(npcEntity, npcEntity, skillCasted, isDefender, isMainTarget);

    public IBattleEntityDump Dump(IMateEntity entity, SkillInfo skillCasted, bool isDefender = false, bool isMainTarget = false)
        => new MateBattleEntityDump(entity, skillCasted, isDefender, isMainTarget);
}