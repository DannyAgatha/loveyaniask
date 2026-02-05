using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Translations;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Managers;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class TranslationsRefreshMessageConsumer : IMessageConsumer<TranslationsRefreshMessage>
    {
        private readonly IForbiddenNamesManager _forbiddenNamesManager;
        private readonly IGameLanguageService _languageService;

        public TranslationsRefreshMessageConsumer(IGameLanguageService languageService, IForbiddenNamesManager forbiddenNamesManager)
        {
            _languageService = languageService;
            _forbiddenNamesManager = forbiddenNamesManager;
        }

        public async Task HandleAsync(TranslationsRefreshMessage notification, CancellationToken token)
        {
            await _forbiddenNamesManager.Reload();
            await _languageService.Reload(notification.IsFullReload);
        }
    }
}