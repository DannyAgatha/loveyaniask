using PhoenixLib.Events;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Configurations.SetEffect;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Items;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Portals;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.Skills;

namespace Plugin.CoreImpl.Maps
{
    public class MapInstanceFactory : IMapInstanceFactory
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
        private readonly IBuffFactory _buffFactory;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly GameMinMaxConfiguration _gameMinMaxConfiguration;
        private readonly IGameLanguageService _languageService;
        private readonly IMeditationManager _meditationManager;
        private readonly IMonsterTalkingConfig _monsterTalkingConfig;
        private readonly IPortalFactory _portalFactory;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ISkillsManager _skillsManager;
        private readonly ISpPartnerConfiguration _spPartnerConfiguration;
        private readonly ISpyOutManager _spyOutManager;
        private readonly ITickManager _tickManager;
        private readonly SerializableGameServer _serializableGameServer;
        private readonly IFishManager _fishManager;
        private readonly ITrainerSpecialistConfiguration _trainerSpecialistConfiguration;
        private readonly PetMaxLevelConfiguration _petMaxLevelConfiguration;
        private readonly IEvtbConfiguration _evtbConfiguration;
        private readonly ILandOfDeathManager _landOfDeathManager;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly FamilyLevelBuffConfiguration _familyLevelBuffConfiguration;
        private readonly PrestigeConfiguration _prestigeConfiguration;

    public MapInstanceFactory(ITickManager tickManager, GameMinMaxConfiguration gameMinMaxConfiguration,
            ISpyOutManager spyOutManager, ISkillsManager skillsManager, IGameLanguageService languageService,
            IMeditationManager meditationManager, IAsyncEventPipeline asyncEventPipeline, IRandomGenerator randomGenerator, IBCardEffectHandlerContainer bCardEffectHandlerContainer,
            IBuffFactory buffFactory, IPortalFactory portalFactory, IGameItemInstanceFactory gameItemInstanceFactory, ISpPartnerConfiguration spPartnerConfiguration,
            IMonsterTalkingConfig monsterTalkingConfig, SerializableGameServer serializableGameServer, IFishManager fishManager, ITrainerSpecialistConfiguration trainerSpecialistConfiguration, 
            PetMaxLevelConfiguration petMaxLevelConfiguration, IEvtbConfiguration evtbConfiguration, ILandOfDeathManager landOfDeathManager, RainbowBattleConfiguration rainbowBattleConfiguration, FamilyLevelBuffConfiguration familyLevelBuffConfiguration, PrestigeConfiguration prestigeConfiguration)
        {
            _tickManager = tickManager;
            _gameMinMaxConfiguration = gameMinMaxConfiguration;
            _spyOutManager = spyOutManager;
            _skillsManager = skillsManager;
            _languageService = languageService;
            _meditationManager = meditationManager;
            _asyncEventPipeline = asyncEventPipeline;
            _randomGenerator = randomGenerator;
            _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
            _buffFactory = buffFactory;
            _portalFactory = portalFactory;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _spPartnerConfiguration = spPartnerConfiguration;
            _monsterTalkingConfig = monsterTalkingConfig;
            _serializableGameServer = serializableGameServer;
            _fishManager = fishManager;
            _trainerSpecialistConfiguration = trainerSpecialistConfiguration;
            _petMaxLevelConfiguration = petMaxLevelConfiguration;
            _evtbConfiguration = evtbConfiguration;
            _landOfDeathManager = landOfDeathManager;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _familyLevelBuffConfiguration = familyLevelBuffConfiguration;
            _prestigeConfiguration = prestigeConfiguration;
        }
    
        public IMapInstance CreateMap(Map map, MapInstanceType mapInstanceType) =>
            new MapInstance(map, mapInstanceType, _tickManager, _gameMinMaxConfiguration, _spyOutManager, _skillsManager, _languageService, _meditationManager,
                _asyncEventPipeline, _randomGenerator, _bCardEffectHandlerContainer, _buffFactory, _portalFactory, _gameItemInstanceFactory, _spPartnerConfiguration, _monsterTalkingConfig, _serializableGameServer, 
                _fishManager, _trainerSpecialistConfiguration, _petMaxLevelConfiguration, _evtbConfiguration, _landOfDeathManager, _rainbowBattleConfiguration, _familyLevelBuffConfiguration, _prestigeConfiguration);
    }
}