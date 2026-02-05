using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class InterChannelChatMessageBroadcastMessageConsumer : IMessageConsumer<InterChannelChatMessageBroadcastMessage>
{
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public InterChannelChatMessageBroadcastMessageConsumer(ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(InterChannelChatMessageBroadcastMessage notification, CancellationToken token)
    {
        _sessionManager.Broadcast(x => x.GenerateSayPacket(_languageService.GetLanguageFormat(notification.DialogKey, x.UserLanguage, notification.Args), notification.ChatMessageColorType));
    }
}