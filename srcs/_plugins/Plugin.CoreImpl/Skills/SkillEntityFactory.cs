// NosEmu
// 


using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;

namespace Plugin.CoreImpl.Skills
{
    public class SkillEntityFactory : IEntitySkillFactory
    {
        private readonly ISkillsManager _skillsManager;

        public SkillEntityFactory(ISkillsManager skillsManager) => _skillsManager = skillsManager;

        public INpcMonsterSkill CreateNpcMonsterSkill(int skillVnum, short rate, bool isBasicAttack, bool isIgnoringHitChance)
        {
            SkillDTO tmp = _skillsManager.GetSkill(skillVnum);
            return tmp == null ? null : new NpcMonsterSkill(tmp, rate, isBasicAttack, isIgnoringHitChance);
        }
    }
}