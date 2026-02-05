using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class InterChannelSendChatMsgByCharIdMessageConsumer : IMessageConsumer<InterChannelSendChatMsgByCharIdMessage>
{
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendChatMsgByCharIdMessageConsumer(ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelSendChatMsgByCharIdMessage e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterId(e.CharacterId);
        localSession?.SendChatMessage(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage), e.ChatMessageColorType);
    }
}