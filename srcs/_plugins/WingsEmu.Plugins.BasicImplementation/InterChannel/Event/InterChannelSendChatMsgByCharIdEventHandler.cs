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

public class InterChannelSendChatMsgByCharIdEventHandler : IAsyncEventProcessor<InterChannelSendChatMsgByCharIdEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelSendChatMsgByCharIdMessage> _messagePublisher;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendChatMsgByCharIdEventHandler(IMessagePublisher<InterChannelSendChatMsgByCharIdMessage> messagePublisher, ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _messagePublisher = messagePublisher;
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelSendChatMsgByCharIdEvent e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterId(e.CharacterId);

        if (localSession == null)
        {
            if (!_sessionManager.IsOnline(e.CharacterId))
            {
                return;
            }

            await _messagePublisher.PublishAsync(new InterChannelSendChatMsgByCharIdMessage
            {
                CharacterId = e.CharacterId,
                DialogKey = e.DialogKey,
                ChatMessageColorType = e.ChatMessageColorType
            }, cancellation);
            return;
        }

        localSession.SendChatMessage(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage), e.ChatMessageColorType);
    }
}