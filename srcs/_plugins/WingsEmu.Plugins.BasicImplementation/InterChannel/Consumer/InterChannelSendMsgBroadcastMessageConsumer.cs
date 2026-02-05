using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class InterChannelSendMsgBroadcastMessageConsumer : IMessageConsumer<InterChannelSendMsgBroadcastMessage>
{
    private readonly ISessionManager _sessionManager;
    private readonly IGameLanguageService _languageService;

    public InterChannelSendMsgBroadcastMessageConsumer(ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public Task HandleAsync(InterChannelSendMsgBroadcastMessage message, CancellationToken cancellation)
    {
        _sessionManager.Broadcast(session =>
        {
            string formattedMessage = _languageService.GetLanguageFormat(
                message.DialogKey,
                session.UserLanguage,
                message.MessageArgs.ToArray()
            );
            return session.GenerateMsgPacket(formattedMessage, message.MessageType);
        });

        return Task.CompletedTask;
    }
}