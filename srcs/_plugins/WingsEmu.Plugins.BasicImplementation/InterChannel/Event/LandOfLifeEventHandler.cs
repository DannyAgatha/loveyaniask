using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event;

public class LandOfLifeEventHandler : IAsyncEventProcessor<LandOfLifeMessageEvent>
{
    private readonly IMessagePublisher<LandOfLifeMessage> _messagePublisher;
    private readonly ISessionManager _serverManager;


    public LandOfLifeEventHandler(IMessagePublisher<LandOfLifeMessage> messagePublisher, ISessionManager serverManager, SerializableGameServer serializableGameServer)
    {
        _messagePublisher = messagePublisher;
        _serverManager = serverManager;
    }

    public async Task HandleAsync(LandOfLifeMessageEvent e, CancellationToken cancellation)
    {
        _serverManager.Broadcast(session => session.GenerateMsgPacket(e.Message, MsgMessageType.Middle));
        _serverManager.Broadcast(session => session.GenerateSayPacket(e.Message, ChatMessageColorType.Yellow));
        
        await _messagePublisher.PublishAsync(new LandOfLifeMessage
        {
            Message = e.Message
        }, cancellation);
    }
}