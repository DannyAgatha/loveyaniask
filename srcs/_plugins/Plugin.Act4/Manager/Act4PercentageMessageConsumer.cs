using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Caligor;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game.Act4.Event;

namespace Plugin.Act4.Manager;

public class Act4PercentageMessageConsumer : IMessageConsumer<Act4PercentageMessage>
{
    private readonly SerializableGameServer _gameServer;
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public Act4PercentageMessageConsumer(SerializableGameServer gameServer, IAsyncEventPipeline asyncEventPipeline)
    {
        _gameServer = gameServer;
        _asyncEventPipeline = asyncEventPipeline;
    }

    public async Task HandleAsync(Act4PercentageMessage notification, CancellationToken token)
    {
        if (_gameServer.ChannelType != GameChannelType.ACT_4)
        {
            return;
        }

        await _asyncEventPipeline.ProcessEventAsync(new Act4PercentageEvent(), token);
    }
}