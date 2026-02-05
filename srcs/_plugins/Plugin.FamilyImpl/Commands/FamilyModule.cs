using System;
using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl.Commands
{
    [Name("family-nostale-ui")]
    [Group("family", "fam")]
    [RequireAuthority(AuthorityType.User)]
    public sealed class FamilyModule : SaltyModuleBase
    {
        private readonly IGameLanguageService _gameLanguage;

        public FamilyModule(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

        [Command("shout")]
        public async Task<SaltyCommandResult> FamilyShoutAsync([Remainder] string message)
        {
            await Context.Player.EmitEventAsync(new FamilyShoutEvent(message));
            return new SaltyCommandResult(true);
        }

        [Command("dismiss", "kick")]
        public async Task<SaltyCommandResult> FamilyDismissAsync([Remainder] string nickname)
        {
            await Context.Player.EmitEventAsync(new FamilyRemoveMemberEvent(nickname));
            return new SaltyCommandResult(true);
        }

        [Command("leave")]
        public async Task<SaltyCommandResult> FamilyLeaveAsync()
        {
            if (!Context.Player.PlayerEntity.IsInFamily())
            {
                return new SaltyCommandResult(false);
            }

            Context.Player.SendQnaiPacket($"gleave", Game18NConstString.LeaveFamilyQuestion);
            return new SaltyCommandResult(true);
        }

        [Command("members")]
        public async Task<SaltyCommandResult> FamilyListMembersAsync()
        {
            await Context.Player.EmitEventAsync(new FamilyListMembersEvent());
            return new SaltyCommandResult(true);
        }

        [Command("notice")]
        public async Task<SaltyCommandResult> FamilyNoticeAsync()
        {
            await Context.Player.EmitEventAsync(new FamilyNoticeMessageEvent(string.Empty, true));
            return new SaltyCommandResult(true);
        }

        [Command("notice")]
        public async Task<SaltyCommandResult> FamilyNoticeAsync([Remainder] string message)
        {
            await Context.Player.EmitEventAsync(new FamilyNoticeMessageEvent(message));
            return new SaltyCommandResult(true);
        }

        [Command("gender")]
        public async Task<SaltyCommandResult> ResetSexAsync(byte gender)
        {
            await Context.Player.EmitEventAsync(new FamilyChangeSexEvent(gender));
            return new SaltyCommandResult(true);
        }

        [Command("title")]
        public async Task<SaltyCommandResult> TitleChangeAsync(string nickname, FamilyTitle familyTitle)
        {
            await Context.Player.EmitEventAsync(new FamilyChangeTitleEvent(nickname, familyTitle));
            return new SaltyCommandResult(true);
        }

        [Command("today")]
        public async Task<SaltyCommandResult> TodayMessageAsync([Remainder] string message)
        {
            await Context.Player.EmitEventAsync(new FamilyTodayEvent(message));
            return new SaltyCommandResult(true);
        }

        [Command("invite")]
        [Description("Invite player to family.")]
        public async Task<SaltyCommandResult> InviteFamilyAsync(
            [Remainder] [Description("Player nickname")]
            string nickname)
        {
            await Context.Player.EmitEventAsync(new FamilySendInviteEvent(nickname));
            return new SaltyCommandResult(true);
        }

        [Command("deputy")]
        public async Task<SaltyCommandResult> FamilyDeputyAsync(string sourceName, [Remainder] string targetName)
        {
            await Context.Player.EmitEventAsync(new FamilyChangeDeputyEvent(sourceName, targetName));
            return new SaltyCommandResult(true);
        }

        [Command("deputy")]
        public async Task<SaltyCommandResult> FamilyDeputyAsync([Remainder] string targetName)
        {
            await Context.Player.EmitEventAsync(new FamilyChangeAuthorityEvent(FamilyAuthority.Deputy, 0, 0, targetName));
            return new SaltyCommandResult(true);
        }

        [Command("head")]
        public async Task<SaltyCommandResult> FamilyHeadAsync([Remainder] string nickname)
        {
            await Context.Player.EmitEventAsync(new FamilyChangeAuthorityEvent(FamilyAuthority.Head, 0, 0, nickname));
            return new SaltyCommandResult(true);
        }

        [Command("keeper", "assistant")]
        public async Task<SaltyCommandResult> FamilyKeeperAsync(string familyKeeperChange, [Remainder] string nickname)
        {
            if (!Enum.TryParse(familyKeeperChange, out FamilyKeeperChange change))
            {
                return new SaltyCommandResult(false);
            }

            FamilyAuthority authority = change switch
            {
                FamilyKeeperChange.Dismiss => FamilyAuthority.Member,
                FamilyKeeperChange.Appointment => FamilyAuthority.Keeper
            };

            await Context.Player.EmitEventAsync(new FamilyChangeAuthorityEvent(authority, 0, 0, nickname));
            return new SaltyCommandResult(true);
        }

        [Command("keeper", "assistant")]
        public async Task<SaltyCommandResult> FamilyKeeperAsync([Remainder] string nickname)
        {
            await Context.Player.EmitEventAsync(new FamilyChangeAuthorityEvent(FamilyAuthority.Keeper, 0, 0, nickname));
            return new SaltyCommandResult(true);
        }

        [Command("disband")]
        public async Task<SaltyCommandResult> FamilyDisbandAsync()
        {
            IClientSession session = Context.Player;
            if (!session.PlayerEntity.IsInFamily())
            {
                return new SaltyCommandResult(false);
            }

            if (session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Head)
            {
                session.SendInfoi(Game18NConstString.OnlyHeadCanDisbandFamily);
                return new SaltyCommandResult(false);
            }

            session.SendQnaiPacket("glrm 1", Game18NConstString.AreYouSureToDisbandFamily);
            return new SaltyCommandResult(true);
        }
    }
}