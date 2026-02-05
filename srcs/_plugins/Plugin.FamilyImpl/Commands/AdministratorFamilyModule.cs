// NosEmu
// 


using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Data.Families;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Plugin.FamilyImpl.Commands
{
    [Name("family-admin")]
    [Group("family", "fam")]
    [RequireAuthority(AuthorityType.GA)]
    public sealed class AdministratorFamilyModule : SaltyModuleBase
    {
        private readonly IFamilyManager _familyManager;

        public AdministratorFamilyModule(IFamilyManager familyManager) => _familyManager = familyManager;

        [Command("showinfo")]
        public async Task<SaltyCommandResult> FamilyShoutAsync([Remainder] string familyName = null)
        {
            IFamily family = Context.Player.PlayerEntity.Family;
            if (!string.IsNullOrEmpty(familyName))
            {
                family = _familyManager.GetFamilyByFamilyName(familyName);
            }

            if (family == null)
            {
                Context.Player.SendErrorChatMessage($"family {familyName} does not exists");
                return new SaltyCommandResult(false);
            }


            Context.Player.SendChatMessage($"[FamilyInfo] : {familyName}", ChatMessageColorType.Green);
            Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
            ISerializer serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            string tmp = serializer.Serialize(family);
            string[] lines = tmp.Split('\n');
            foreach (string line in lines)
            {
                Context.Player.SendChatMessage($"{line}", ChatMessageColorType.Green);
            }

            Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
            return new SaltyCommandResult(true);
        }


        [Command("create")]
        public async Task<SaltyCommandResult> CreateFamilyAsync(string name)
        {
            await Context.Player.EmitEventAsync(new FamilyCreateEvent
            {
                Name = name
            });
            return new SaltyCommandResult(true);
        }

        [Command("logs-addxp")]
        public async Task<SaltyCommandResult> GenerateFamilyLog()
        {
            await Context.Player.EmitEventAsync(new FamilyAddLogEvent(new FamilyLogDto
            {
                FamilyLogType = FamilyLogType.FamilyXP,
                Actor = Context.Player.PlayerEntity.Name,
                Argument1 = 100.ToString()
            }));
            return new SaltyCommandResult(true);
        }

        [Command("logs-addlevelup")]
        public async Task<SaltyCommandResult> GenerateFamilyLog2()
        {
            await Context.Player.EmitEventAsync(new FamilyAddLogEvent(new FamilyLogDto
            {
                FamilyLogType = FamilyLogType.FamilyLevelUp,
                Argument1 = 2.ToString()
            }));
            return new SaltyCommandResult(true);
        }

        [Command("addxp")]
        public async Task<SaltyCommandResult> ObtainFamilyXp(int xp)
        {
            await Context.Player.EmitEventAsync(new FamilyAddExperienceEvent(xp, FamXpObtainedFromType.Command));
            return new SaltyCommandResult(true, $"[FAMILY] Adding {xp} xp");
        }

        [Command("set-level", "level")]
        public async Task<SaltyCommandResult> SetFamilyLevel() =>
            // todo implementation on microservice
            new SaltyCommandResult(true);
    }
}