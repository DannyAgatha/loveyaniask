using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsAPI.Communication.Families;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Families;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl.Achievements
{
    [Name("family-achievements-admin")]
    [Group("family", "fam")]
    [RequireAuthority(AuthorityType.GA)]
    public sealed class AdministratorFamilyModule : SaltyModuleBase
    {
        [Group("mission", "missions", "ms")]
        public sealed class MissionsModule : SaltyModuleBase
        {
            private readonly FamilyMissionsConfiguration _config;
            private readonly IFamilyMissionManager _familyAchievementManager;
            private readonly IMessagePublisher<FamilyMissionsResetMessage> _message;

            public MissionsModule(FamilyMissionsConfiguration config, IFamilyMissionManager familyAchievementManager, IMessagePublisher<FamilyMissionsResetMessage> message)
            {
                _config = config;
                _familyAchievementManager = familyAchievementManager;
                _message = message;
            }

            [Command("reset")]
            public async Task<SaltyCommandResult> Reset()
            {
                await _message.PublishAsync(new FamilyMissionsResetMessage());
                return new SaltyCommandResult(true);
            }

            [Command("unlock")]
            public async Task<SaltyCommandResult> UnlockAchievement([Description("Missions' Id")] int missionId)
            {
                IFamily family = Context.Player.PlayerEntity.Family;
                if (family == null)
                {
                    return new SaltyCommandResult(false, "You don't have a family");
                }

                FamilyMissionSpecificConfiguration? tmp = _config.FirstOrDefault(s => s.MissionId == missionId);
                if (tmp == null)
                {
                    return new SaltyCommandResult(false, $"family mission {missionId} configuration not found");
                }

                _familyAchievementManager.IncrementFamilyMission(family.Id, Context.Player.PlayerEntity.Id, missionId, tmp.Value);
                return new SaltyCommandResult(true, $"Mission {missionId} will be unlocked soon");
            }


            [Command("add")]
            public async Task<SaltyCommandResult> AddCounterToAchievement([Description("Missions' Id")] int missionId, int counter)
            {
                IFamily family = Context.Player.PlayerEntity.Family;
                if (family == null)
                {
                    return new SaltyCommandResult(false, "You don't have a family");
                }

                _familyAchievementManager.IncrementFamilyMission(family.Id, Context.Player.PlayerEntity.Id, missionId, counter);
                return new SaltyCommandResult(true, $"Mission {missionId} will have {counter} increments added soon");
            }
        }

        [Group("achievement", "achievements", "ac")]
        public sealed class AchievementModule : SaltyModuleBase
        {
            private readonly FamilyAchievementsConfiguration _config;
            private readonly IFamilyAchievementManager _familyAchievementManager;

            public AchievementModule(FamilyAchievementsConfiguration config, IFamilyAchievementManager familyAchievementManager)
            {
                _config = config;
                _familyAchievementManager = familyAchievementManager;
            }

            [Command("unlock")]
            public async Task<SaltyCommandResult> UnlockAchievement(int achievementId)
            {
                IFamily family = Context.Player.PlayerEntity.Family;
                if (family == null)
                {
                    return new SaltyCommandResult(false, "You don't have a family");
                }

                FamilyAchievementSpecificConfiguration? tmp = _config.Counters.FirstOrDefault(s => s.Id == achievementId);
                if (tmp == null)
                {
                    return new SaltyCommandResult(false, $"family achievement {achievementId} configuration not found");
                }

                _familyAchievementManager.IncrementFamilyAchievement(family.Id, achievementId, tmp.Value);
                return new SaltyCommandResult(true, $"{achievementId} will be unlocked soon");
            }

            [Command("add")]
            public async Task<SaltyCommandResult> AddCounterToAchievement(int achievementId, int counter)
            {
                IFamily family = Context.Player.PlayerEntity.Family;
                if (family == null)
                {
                    return new SaltyCommandResult(false, "You don't have a family");
                }

                _familyAchievementManager.IncrementFamilyAchievement(family.Id, achievementId, counter);
                return new SaltyCommandResult(true);
            }

            [Command("add")]
            public async Task<SaltyCommandResult> AddCounterToAchievement(IClientSession target, int achievementId, int counter)
            {
                IFamily family = target.PlayerEntity.Family;
                if (family == null)
                {
                    return new SaltyCommandResult(false, "Player doesn't have a family");
                }

                _familyAchievementManager.IncrementFamilyAchievement(family.Id, achievementId, counter);
                return new SaltyCommandResult(true);
            }
        }
    }
}