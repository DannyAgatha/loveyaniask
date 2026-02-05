using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event;

public class InterChannelSendChatMsgByNicknameEventHandler : IAsyncEventProcessor<InterChannelSendChatMsgByNicknameEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelSendChatMsgByNicknameMessage> _messagePublisher;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendChatMsgByNicknameEventHandler(ISessionManager sessionManager, IGameLanguageService languageService,
        IMessagePublisher<InterChannelSendChatMsgByNicknameMessage> messagePublisher)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(InterChannelSendChatMsgByNicknameEvent e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterName(e.Nickname);

        if (localSession == null)
        {
            if (!_sessionManager.IsOnline(e.Nickname))
            {
                return;
            }

            await _messagePublisher.PublishAsync(new InterChannelSendChatMsgByNicknameMessage
            {
                Nickname = e.Nickname,
                DialogKey = e.DialogKey,
                ChatMessageColorType = e.ChatMessageColorType
            }, cancellation);
            return;
        }

        localSession.SendChatMessage(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage), e.ChatMessageColorType);
    }
}