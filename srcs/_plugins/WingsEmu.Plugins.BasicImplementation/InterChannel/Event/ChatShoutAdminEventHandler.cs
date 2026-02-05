using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event;

public class ChatShoutAdminEventHandler : IAsyncEventProcessor<ChatShoutAdminEvent>
{
    private readonly IMessagePublisher<ChatShoutAdminMessage> _messagePublisher;
    private readonly ISessionManager _serverManager;

    public ChatShoutAdminEventHandler(IMessagePublisher<ChatShoutAdminMessage> messagePublisher, ISessionManager serverManager)
    {
        _messagePublisher = messagePublisher;
        _serverManager = serverManager;
    }

    public async Task HandleAsync(ChatShoutAdminEvent e, CancellationToken cancellation)
    {
        string msg = e.Sender.GenerateMsgPacket(e.Message, MsgMessageType.MiddleYellow);
        _serverManager.Broadcast(x => msg);

        string message = e.Message;
        _serverManager.Broadcast(session => session.GenerateSayPacket($"({session.GetLanguage(GameDialogKey.ADMIN_BROADCAST_CHATMESSAGE_SENDER)}): {message}", ChatMessageColorType.Yellow));
        await _messagePublisher.PublishAsync(new ChatShoutAdminMessage
        {
            Message = e.Message
        }, cancellation);
    }
}