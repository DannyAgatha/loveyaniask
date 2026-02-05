using System;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsAPI.Communication.Translations;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("administrator-language")]
[Description("Module related to Administrator commands.")]
[Group("translations", "i18n", "language")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorLanguageModule : SaltyModuleBase
{
    private readonly IForbiddenNamesManager _forbiddenNamesManager;
    private readonly IGameLanguageService _language;
    private readonly IMessagePublisher<TranslationsRefreshMessage> _messagePublisher;

    public AdministratorLanguageModule(IGameLanguageService language, IMessagePublisher<TranslationsRefreshMessage> messagePublisher, IForbiddenNamesManager forbiddenNamesManager)
    {
        _language = language;
        _messagePublisher = messagePublisher;
        _forbiddenNamesManager = forbiddenNamesManager;
    }


    [Command("verify", "check")]
    public async Task<SaltyCommandResult> CheckTranslations()
    {
        foreach (GameDialogKey enumValue in Enum.GetValues(typeof(GameDialogKey)))
        {
            GameDialogKey enm = enumValue;
            string notTranslated = $"#{enm.ToString()}";

            string translated = _language.GetLanguage(enumValue, Context.Player.UserLanguage);
            if (translated != notTranslated)
            {
                continue;
            }

            Context.Player.SendErrorChatMessage($"[{Context.Player.UserLanguage.ToString()}] {enumValue.ToString()} - not translated");
        }

        return new SaltyCommandResult(true);
    }


    [Command("reload", "refresh")]
    public async Task<SaltyCommandResult> ReloadTranslations(bool isFull = false, bool forAllChannels = false)
    {
        await _forbiddenNamesManager.Reload();
        await _language.Reload(isFull);
        if (forAllChannels)
        {
            await _messagePublisher.PublishAsync(new TranslationsRefreshMessage
            {
                IsFullReload = isFull
            });
        }

        return new SaltyCommandResult(true, "Translations reloaded");
    }
}