using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event;

public class InterChannelChatMessageBroadcastEventHandler : IAsyncEventProcessor<InterChannelChatMessageBroadcastEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelChatMessageBroadcastMessage> _messagePublisher;
    private readonly ISessionManager _sessionManager;

    public InterChannelChatMessageBroadcastEventHandler(ISessionManager sessionManager, IGameLanguageService languageService,
        IMessagePublisher<InterChannelChatMessageBroadcastMessage> messagePublisher)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(InterChannelChatMessageBroadcastEvent e, CancellationToken cancellation)
    {
        await _messagePublisher.PublishAsync(new InterChannelChatMessageBroadcastMessage
        {
            Args = e.Args,
            ChatMessageColorType = e.ChatMessageColorType,
            DialogKey = e.DialogKey
        }, cancellation);
        _sessionManager.Broadcast(x => x.GenerateSayPacket(_languageService.GetLanguageFormat(e.DialogKey, x.UserLanguage, e.Args), e.ChatMessageColorType));
    }
}