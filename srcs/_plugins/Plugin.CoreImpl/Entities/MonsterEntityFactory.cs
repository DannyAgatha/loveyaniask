using System.Linq;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Skills;
using WingsEmu.Game.Triggers;

namespace Plugin.CoreImpl.Entities
{
    public class MonsterEntityFactory : IMonsterEntityFactory
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IBattleEntityAlgorithmService _battleEntityAlgorithmService;
        private readonly INpcMonsterManager _npcMonsterManager;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IEntitySkillFactory _skillFactory;
        private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;
        public MonsterEntityFactory(INpcMonsterManager npcMonsterManager, IAsyncEventPipeline asyncEventPipeline, IRandomGenerator randomGenerator, IEntitySkillFactory skillFactory,
            IBattleEntityAlgorithmService battleEntityAlgorithmService, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
        {
            _npcMonsterManager = npcMonsterManager;
            _asyncEventPipeline = asyncEventPipeline;
            _randomGenerator = randomGenerator;
            _skillFactory = skillFactory;
            _battleEntityAlgorithmService = battleEntityAlgorithmService;
            _characterSizeModifiersConfiguration  = characterSizeModifiersConfiguration;
        }

        public IMonsterEntity CreateMapMonster(MapMonsterDTO mapMonsterDto, IMapInstance mapInstance)
        {
            IMonsterData monsterData = _npcMonsterManager.GetNpc(mapMonsterDto.MonsterVNum);
            if (monsterData == null)
            {
                return null;
            }

            var skills = monsterData.MonsterSkills.Select(skill => _skillFactory.CreateNpcMonsterSkill(skill.Skill.Id, skill.Rate, skill.IsBasicAttack, skill.IsIgnoringHitChance)).ToList();
            // this is pure puke, needs to rework NpcMonster properly...
            if (monsterData is NpcMonsterDto npcMonsterDto)
            {
                skills.AddRange(npcMonsterDto.Skills.Select(s => _skillFactory.CreateNpcMonsterSkill(s.SkillVNum, s.Rate, s.IsBasicAttack, s.IsIgnoringHitChance)));
            }

            // fetch skill

            var monsterBuilder = new MonsterEntityBuilder
            {
                IsWalkingAround = mapMonsterDto.IsMoving,
                IsRespawningOnDeath = true,
                IsHostile = (HostilityType)monsterData.RawHostility != HostilityType.NOT_HOSTILE,
                PositionX = mapMonsterDto.MapX,
                PositionY = mapMonsterDto.MapY,
                Direction = mapMonsterDto.Direction
            };

            return new MonsterEntity(mapInstance.GenerateEntityId(), new EventTriggerContainer(_asyncEventPipeline), new BCardComponent(_randomGenerator),
                _asyncEventPipeline, monsterData, mapInstance, monsterBuilder, skills, _battleEntityAlgorithmService, _characterSizeModifiersConfiguration);
        }

        public IMonsterEntity CreateMonster(int monsterVNum, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null)
        {
            IMonsterData monsterData = _npcMonsterManager.GetNpc(monsterVNum);
            return monsterData == null ? null : CreateMonster(monsterData, mapInstance, monsterAdditionalData);
        }

        public IMonsterEntity CreateMonster(int? id, int monsterVNum, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null)
        {
            IMonsterData monsterData = _npcMonsterManager.GetNpc(monsterVNum);
            return monsterData == null ? null : CreateMonster(id, monsterData, mapInstance, monsterAdditionalData);
        }

        public IMonsterEntity CreateMonster(IMonsterData monsterData, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null) =>
            CreateMonster(mapInstance.GenerateEntityId(), monsterData, mapInstance, monsterAdditionalData);

        public IMonsterEntity CreateMonster(int? id, IMonsterData monsterData, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null)
        {
            var skills = monsterData.MonsterSkills.Select(skill => _skillFactory.CreateNpcMonsterSkill(skill.Skill.Id, skill.Rate, skill.IsBasicAttack, skill.IsIgnoringHitChance)).ToList();

            // this is pure puke, needs to rework NpcMonster properly...
            if (monsterData is NpcMonsterDto npcMonsterDto)
            {
                skills.AddRange(npcMonsterDto.Skills.Select(s => _skillFactory.CreateNpcMonsterSkill(s.SkillVNum, s.Rate, s.IsBasicAttack, s.IsIgnoringHitChance)));
            }

            // get skills
            // get drops
            // get bcards
            return new MonsterEntity(id ?? mapInstance.GenerateEntityId(), new EventTriggerContainer(_asyncEventPipeline),
                new BCardComponent(_randomGenerator), _asyncEventPipeline, monsterData, mapInstance, monsterAdditionalData, skills, _battleEntityAlgorithmService, _characterSizeModifiersConfiguration);
        }
    }
}