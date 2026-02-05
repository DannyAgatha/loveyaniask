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

public class InterChannelSendInfoByCharIdEventHandler : IAsyncEventProcessor<InterChannelSendInfoByCharIdEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelSendInfoByCharIdMessage> _messagePublisher;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendInfoByCharIdEventHandler(IMessagePublisher<InterChannelSendInfoByCharIdMessage> messagePublisher, ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _messagePublisher = messagePublisher;
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelSendInfoByCharIdEvent e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterId(e.CharacterId);

        if (localSession == null)
        {
            if (!_sessionManager.IsOnline(e.CharacterId))
            {
                return;
            }

            await _messagePublisher.PublishAsync(new InterChannelSendInfoByCharIdMessage
            {
                CharacterId = e.CharacterId,
                DialogKey = e.DialogKey
            }, cancellation);
            return;
        }

        localSession.SendInfo(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage));
    }
}