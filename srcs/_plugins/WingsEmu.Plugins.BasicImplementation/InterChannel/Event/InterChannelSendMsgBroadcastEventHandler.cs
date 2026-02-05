using System.Linq;
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

public class InterChannelSendMsgBroadcastEventHandler : IAsyncEventProcessor<InterChannelSendMsgBroadcastEvent>
{
    private readonly ISessionManager _sessionManager;
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelSendMsgBroadcastMessage> _messagePublisher;

    public InterChannelSendMsgBroadcastEventHandler(
        ISessionManager sessionManager,
        IGameLanguageService languageService,
        IMessagePublisher<InterChannelSendMsgBroadcastMessage> messagePublisher)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(InterChannelSendMsgBroadcastEvent e, CancellationToken cancellation)
    {
        _sessionManager.Broadcast(session =>
        {
            string message = _languageService.GetLanguageFormat(e.DialogKey, session.UserLanguage, e.MessageArgs?.Cast<object>().ToArray() ?? []);

            return session.GenerateMsgPacket(message, e.MessageType);
        });


        await _messagePublisher.PublishAsync(new InterChannelSendMsgBroadcastMessage
        {
            DialogKey = e.DialogKey,
            MessageType = e.MessageType
        }, cancellation);
    }
}