using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class InterChannelSendChatMsgByNicknameMessageConsumer : IMessageConsumer<InterChannelSendChatMsgByNicknameMessage>
{
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendChatMsgByNicknameMessageConsumer(ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelSendChatMsgByNicknameMessage e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterName(e.Nickname);
        localSession?.SendChatMessage(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage), e.ChatMessageColorType);
    }
}