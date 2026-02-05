using WingsEmu.Game._i18n;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.MinilandExtensions
{
    public static class MinilandUtilitiesExtensions
    {
        public static string GetMinilandSerializedMessage(this IClientSession session, IGameLanguageService languageService) =>
            session.GetMinilandCleanMessage(languageService).Replace(' ', '^');

        public static string GetMinilandCleanMessage(this IClientSession session, IGameLanguageService languageService) =>
            session.PlayerEntity.MinilandMessage == string.Empty ? languageService.GetLanguage(GameDialogKey.MINILAND_WELCOME_MESSAGE, session.UserLanguage) : session.PlayerEntity.MinilandMessage;
    }
}