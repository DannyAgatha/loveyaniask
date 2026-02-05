using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class LandOfLifeMessageConsumer : IMessageConsumer<LandOfLifeMessage>
{
    private readonly ISessionManager _serverManager;

    public LandOfLifeMessageConsumer(ISessionManager serverManager) => _serverManager = serverManager;

    public async Task HandleAsync(LandOfLifeMessage notification, CancellationToken token)
    {
        string message = notification.Message;
        
        _serverManager.Broadcast(session => session.GenerateMsgPacket(message, MsgMessageType.MiddleYellow));
        _serverManager.Broadcast(session => session.GenerateSayPacket(message, ChatMessageColorType.Yellow));
    }
}