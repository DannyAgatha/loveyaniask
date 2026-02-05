using System.Linq;
using PhoenixLib.Events;
using Plugin.CoreImpl.Configs;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Triggers;

namespace Plugin.CoreImpl.Entities
{
    public class NpcEntityFactory : INpcEntityFactory
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly HardcodedDialogsByNpcVnumFileConfig _config;
        private readonly IMapNpcManager _mapNpcManager;
        private readonly INpcMonsterManager _npcMonsterManager;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IShopManager _shopManager;
        private readonly ISkillsManager _skillsManager;
        private readonly CharacterSizeModifiersConfiguration  _characterSizeModifiersConfiguration;
        public NpcEntityFactory(IAsyncEventPipeline asyncEventPipeline, IRandomGenerator randomGenerator, INpcMonsterManager npcMonsterManager, IMapNpcManager mapNpcManager, IShopManager shopManager,
            HardcodedDialogsByNpcVnumFileConfig config, ISkillsManager skillsManager, CharacterSizeModifiersConfiguration  characterSizeModifiersConfiguration)
        {
            _asyncEventPipeline = asyncEventPipeline;
            _randomGenerator = randomGenerator;
            _npcMonsterManager = npcMonsterManager;
            _mapNpcManager = mapNpcManager;
            _shopManager = shopManager;
            _config = config;
            _skillsManager = skillsManager;
            _characterSizeModifiersConfiguration = characterSizeModifiersConfiguration;
        }

        public INpcEntity CreateMapNpc(int monsterVNum, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null)
        {
            MapNpcDTO npcDto = _mapNpcManager.GetMapNpcsPerVNum(monsterVNum).FirstOrDefault();
            return npcDto == null ? null : CreateMapNpc(npcDto, mapInstance, id, npcAdditionalData);
        }

        public INpcEntity CreateMapNpc(IMonsterData monsterDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null)
        {
            MapNpcDTO npcDto = _mapNpcManager.GetMapNpcsPerVNum(monsterDto.MonsterVNum).FirstOrDefault();
            return npcDto == null ? null : CreateMapNpc(monsterDto, npcDto, mapInstance, id, npcAdditionalData);
        }

        public INpcEntity CreateMapNpc(MapNpcDTO npcDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null)
        {
            IMonsterData monsterData = _npcMonsterManager.GetNpc(npcDto.NpcVNum);
            return monsterData == null ? null : CreateMapNpc(monsterData, npcDto, mapInstance, id, npcAdditionalData);
        }

        public INpcEntity CreateMapNpc(IMonsterData monsterData, MapNpcDTO npcDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null)
        {
            ShopNpc shop = _shopManager.GetShopByNpcId(npcDto.Id);
            return new NpcEntity(new EventTriggerContainer(_asyncEventPipeline), new BCardComponent(_randomGenerator), _asyncEventPipeline, monsterData, mapInstance, _skillsManager, _characterSizeModifiersConfiguration, npcDto, shop, id,
                npcAdditionalData);
        }

        public INpcEntity CreateNpc(int monsterVNum, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null)
        {
            IMonsterData monsterData = _npcMonsterManager.GetNpc(monsterVNum);
            if (monsterData == null)
            {
                // should throw;
                return null;
            }

            return CreateNpc(monsterData, mapInstance, id, npcAdditionalData);
        }

        public INpcEntity CreateNpc(IMonsterData monsterDto, IMapInstance mapInstance, int? id = null, INpcAdditionalData npcAdditionalData = null)
        {
            HardcodedDialogsByNpcVnum tmp = _config.FirstOrDefault(s => s.NpcVnum == monsterDto.MonsterVNum);
            return new NpcEntity(new EventTriggerContainer(_asyncEventPipeline), new BCardComponent(_randomGenerator), _asyncEventPipeline, monsterDto, mapInstance, _skillsManager, _characterSizeModifiersConfiguration,null, null, id,
                npcAdditionalData, tmp?.DialogId);
        }
    }
}