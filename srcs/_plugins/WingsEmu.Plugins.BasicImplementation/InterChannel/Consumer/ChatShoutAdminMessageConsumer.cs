using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class ChatShoutAdminMessageConsumer : IMessageConsumer<ChatShoutAdminMessage>
{
    private readonly ISessionManager _serverManager;

    public ChatShoutAdminMessageConsumer(ISessionManager serverManager) => _serverManager = serverManager;

    public async Task HandleAsync(ChatShoutAdminMessage notification, CancellationToken token)
    {
        string message = notification.Message;

        string msg = ((IClientSession)null).GenerateMsgPacket(message, MsgMessageType.MiddleYellow);
        _serverManager.Broadcast(x => msg);

        _serverManager.Broadcast(session => session.GenerateSayPacket($"({session.GetLanguage(GameDialogKey.ADMIN_BROADCAST_CHATMESSAGE_SENDER)}): {message}", ChatMessageColorType.Yellow));
    }
}