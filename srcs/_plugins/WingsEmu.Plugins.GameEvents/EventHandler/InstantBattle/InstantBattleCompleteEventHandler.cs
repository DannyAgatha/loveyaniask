using System.Collections.Generic;
using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.GameEvent.InstantBattle;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;
using WingsEmu.Plugins.GameEvents.DataHolder;
using WingsEmu.Plugins.GameEvents.Event.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.EventHandler.InstantBattle
{
    public class InstantBattleCompleteEventHandler : IAsyncEventProcessor<InstantBattleCompleteEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly GameMinMaxConfiguration _minMaxConfiguration;
        private readonly IPortalFactory _portalFactory;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;

        public InstantBattleCompleteEventHandler(IGameLanguageService gameLanguage, GameMinMaxConfiguration minMaxConfiguration, IReputationConfiguration reputationConfiguration,
            IPortalFactory portalFactory, IRankingManager rankingManager, BattlePassQuestConfiguration battlePassQuestConfiguration)
        {
            _gameLanguage = gameLanguage;
            _minMaxConfiguration = minMaxConfiguration;
            _reputationConfiguration = reputationConfiguration;
            _portalFactory = portalFactory;
            _rankingManager = rankingManager;
            _battlePassQuestConfiguration = battlePassQuestConfiguration;
        }

        public async Task HandleAsync(InstantBattleCompleteEvent e, CancellationToken cancellation)
        {
            IMapInstance map = e.Instance.MapInstance;
            InstantBattleReward reward = e.Instance.InternalConfiguration.Reward;
            InstantBattleInstance instance = e.Instance;

            e.Instance.Finished = true;

            await map.BroadcastAsync(async x => x.GenerateMsgiPacket(MessageType.Default, Game18NConstString.InstantCombatSuccess));

            foreach (IClientSession session in map.Sessions)
            {
                IPlayerEntity character = session.PlayerEntity;
                bool isHeroic = character.HeroLevel > 0;
                int levelFactor = isHeroic ? character.HeroLevel : character.Level;

                long gold = reward.GoldMultiplier * levelFactor;
                long reputation = reward.ReputationMultiplier * levelFactor;
                int specialistPoint = reward.SpPointsMultiplier * levelFactor;
                int familyExperience = reward.FamilyExperience;
                int dignity = reward.Dignity;

                await session.EmitEventAsync(new InstantBattleWonEvent());
                await session.EmitEventAsync(new GenerateReputationEvent
                {
                    Amount = (int)reputation,
                    SendMessage = true
                });
                await session.EmitEventAsync(new GenerateGoldEvent(gold, false, false));
                session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.VictoryReceivedGold, 4, (int)gold);
                session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.VictoryReceivedFame, 4, (int)reputation);
                character.SpPointsBonus += specialistPoint;
                character.AddDignity(dignity, _minMaxConfiguration, _gameLanguage, _reputationConfiguration, _rankingManager.TopReputation);
                session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.VictoryReceivedAdditionalSP, 4, specialistPoint);
                if (character.Family != null)
                {
                    await character.Session.EmitEventAsync(new FamilyAddExperienceEvent(familyExperience, FamXpObtainedFromType.InstantCombat));
                }
                
                int baseExperience = session.PlayerEntity.SubClass.IsPveSubClass() ? 500 : session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 250 : 0;
                session.AddTierExperience(baseExperience, _gameLanguage, false);

                character.Hp = session.PlayerEntity.MaxHp;
                character.Mp = session.PlayerEntity.MaxMp;

                if (character.SpPointsBonus > _minMaxConfiguration.MaxSpAdditionalPoints)
                {
                    character.SpPointsBonus = _minMaxConfiguration.MaxSpAdditionalPoints;
                }

                session.RefreshSpPoint();
                session.RefreshStat();
                session.RefreshStatInfo();
                // await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXInstantCombat));
            }

            var pos = new Position(e.Instance.InternalConfiguration.ReturnPortalX, e.Instance.InternalConfiguration.ReturnPortalY);
            IPortalEntity portal = _portalFactory.CreatePortal(PortalType.TSNormal, map, pos, map, pos);
            map.AddPortalToMap(portal);
        }
    }
}