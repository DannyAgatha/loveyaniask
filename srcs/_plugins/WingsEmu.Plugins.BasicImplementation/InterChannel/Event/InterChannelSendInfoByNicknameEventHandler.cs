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

public class InterChannelSendInfoByNicknameEventHandler : IAsyncEventProcessor<InterChannelSendInfoByNicknameEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMessagePublisher<InterChannelSendInfoByNicknameMessage> _messagePublisher;
    private readonly ISessionManager _sessionManager;

    public InterChannelSendInfoByNicknameEventHandler(IMessagePublisher<InterChannelSendInfoByNicknameMessage> messagePublisher, ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _messagePublisher = messagePublisher;
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelSendInfoByNicknameEvent e, CancellationToken cancellation)
    {
        IClientSession localSession = _sessionManager.GetSessionByCharacterName(e.Nickname);

        if (localSession == null)
        {
            if (!_sessionManager.IsOnline(e.Nickname))
            {
                return;
            }

            await _messagePublisher.PublishAsync(new InterChannelSendInfoByNicknameMessage
            {
                Nickname = e.Nickname,
                DialogKey = e.DialogKey
            }, cancellation);
            return;
        }

        localSession.SendInfo(_languageService.GetLanguage(e.DialogKey, localSession.UserLanguage));
    }
}