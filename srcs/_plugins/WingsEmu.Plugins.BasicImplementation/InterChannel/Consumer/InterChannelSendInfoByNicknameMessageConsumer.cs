using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class InterChannelSendInfoByNicknameMessageConsumer : IMessageConsumer<InterChannelSendInfoByNicknameMessage>
{
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendInfoByNicknameMessageConsumer(ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelSendInfoByNicknameMessage e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterName(e.Nickname);
        localSession?.SendInfo(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage));
    }
}