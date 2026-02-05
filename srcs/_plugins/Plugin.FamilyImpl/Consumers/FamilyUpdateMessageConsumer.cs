using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyUpdateMessageConsumer : IMessageConsumer<FamilyUpdateMessage>
    {
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyManager _familyManager;
        private readonly IRankingManager _rankingManager;
        private readonly IItemsManager _itemsManager;
        private readonly IGameLanguageService _languageService;
        private readonly IMessagePublisher<FamilyShoutMessage> _messagePublisher;
        private readonly ISessionManager _sessionManager;

        public FamilyUpdateMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager, IGameLanguageService languageService, FamilyConfiguration familyConfiguration,
            IMessagePublisher<FamilyShoutMessage> messagePublisher, IItemsManager itemsManager, IRankingManager rankingManager)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
            _languageService = languageService;
            _familyConfiguration = familyConfiguration;
            _messagePublisher = messagePublisher;
            _itemsManager = itemsManager;
            _rankingManager = rankingManager;
        }

        public async Task HandleAsync(FamilyUpdateMessage e, CancellationToken cancellation)
        {
            foreach (FamilyDTO familyDto in e.Families)
            {
                Family family = _familyManager.GetFamilyByFamilyIdCache(familyDto.Id);
                if (family == null)
                {
                    continue;
                }

                switch (e.ChangedInfoFamilyUpdate)
                {
                    case ChangedInfoFamilyUpdate.Experience:
                        HandleFamilyExperience(family, familyDto);
                        break;
                    case ChangedInfoFamilyUpdate.Notice:
                        HandleFamilyNotice(family, familyDto);
                        break;
                    case ChangedInfoFamilyUpdate.HeadSex:
                        await HandleHeadSex(family, familyDto);
                        break;
                    case ChangedInfoFamilyUpdate.Settings:
                        HandleSettings(family, familyDto);
                        break;
                    case ChangedInfoFamilyUpdate.Upgrades:
                        HandleFamilyUpgradeUpdate(family, familyDto);
                        break;
                    case ChangedInfoFamilyUpdate.AchievementsAndMissions:
                        HandleFamilyAchievementsUpdate(family, familyDto);
                        break;
                }
            }
        }

        private void HandleSettings(Family family, FamilyDTO familyDto)
        {
            family.AssistantWarehouseAuthorityType = familyDto.AssistantWarehouseAuthorityType;
            family.AssistantCanInvite = familyDto.AssistantCanInvite;
            family.AssistantCanGetHistory = familyDto.AssistantCanGetHistory;
            family.AssistantCanNotice = familyDto.AssistantCanNotice;
            family.AssistantCanShout = familyDto.AssistantCanShout;

            family.MemberWarehouseAuthorityType = familyDto.MemberWarehouseAuthorityType;
            family.MemberCanGetHistory = familyDto.MemberCanGetHistory;

            FamilyPacketExtensions.SendFamilyInfoToMembers(family, _sessionManager, _familyConfiguration);
        }

        private async Task HandleHeadSex(Family family, FamilyDTO familyDto)
        {
            family.HeadGender = familyDto.HeadGender;
            FamilyPacketExtensions.SendFamilyInfoToMembers(family, _sessionManager, _familyConfiguration);

            await _messagePublisher.PublishAsync(new FamilyShoutMessage
            {
                FamilyId = family.Id,
                GameDialogKey = GameDialogKey.FAMILY_SHOUTMESSAGE_HEAD_CHANGE_SEX
            });
        }

        private void HandleFamilyNotice(Family family, FamilyDTO familyDto)
        {
            family.Message = familyDto.Message;
            FamilyPacketExtensions.SendFamilyNoticeMessage(family, _sessionManager, _familyConfiguration);
        }

        private void HandleFamilyExperience(Family family, FamilyDTO familyDto)
        {
            family.Experience = familyDto.Experience;
            if (family.Level == familyDto.Level)
            {
                FamilyPacketExtensions.SendFamilyInfoToMembers(family, _sessionManager, _familyConfiguration);
                return;
            }

            /*_familyManager.SendLogToFamilyServer(new FamilyLogDto
            {
                Actor = familyDto.Level.ToString(),
                FamilyId = familyDto.Id,
                FamilyLogType = FamilyLogType.FamilyLevelUp,
                Timestamp = DateTime.UtcNow
            });*/

            family.Level = familyDto.Level;
            FamilyPacketExtensions.SendFamilyLevelUpMessageToMembers(family, _sessionManager, _languageService, _familyConfiguration);
        }

        private void HandleFamilyUpgradeUpdate(Family family, FamilyDTO familyDto)
        {
            familyDto.Upgrades ??= new FamilyUpgradeDto();
            familyDto.Upgrades.UpgradesBought ??= new HashSet<int>();
            familyDto.Upgrades.UpgradeValues ??= new Dictionary<FamilyUpgradeType, (short, short?)>();

            foreach (int upgradeId in familyDto.Upgrades.UpgradesBought)
            {
                if (family.Upgrades.ContainsKey(upgradeId))
                {
                    continue;
                }

                family.Upgrades.Add(upgradeId, new FamilyUpgrade
                {
                    Id = upgradeId,
                    State = FamilyUpgradeState.PASSIVE
                });
            }

            foreach ((FamilyUpgradeType upgradeType, (short value, short? secondValue)) in familyDto.Upgrades.UpgradeValues)
            {
                family.UpgradeValues[upgradeType] = (value, secondValue);
            }

            family.SendFmpPacket(_sessionManager, _itemsManager, _familyManager, _rankingManager, _familyConfiguration);
        }

        private void HandleFamilyAchievementsUpdate(Family family, FamilyDTO familyDto)
        {
            if (familyDto.Achievements?.Achievements != null)
            {
                family.Achievements.Clear();
                foreach ((int achievementId, FamilyAchievementCompletionDto achievement) in familyDto.Achievements.Achievements)
                {
                    family.Achievements[achievementId] = achievement;
                }
            }

            if (familyDto.Achievements?.Progress != null)
            {
                family.AchievementProgress.Clear();
                foreach ((int achievementId, FamilyAchievementProgressDto achievement) in familyDto.Achievements.Progress)
                {
                    family.AchievementProgress[achievementId] = achievement;
                }
            }

            if (familyDto.Missions?.Missions != null)
            {
                family.Mission.Clear();
                foreach ((int achievementId, FamilyMissionDto achievement) in familyDto.Missions.Missions)
                {
                    family.Mission[achievementId] = achievement;
                }
            }

            family.SendFmiPacket(_sessionManager);
        }
    }
}