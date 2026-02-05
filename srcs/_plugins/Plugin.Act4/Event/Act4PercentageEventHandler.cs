using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace Plugin.Act4.Event;

public class Act4PercentageEventHandler : IAsyncEventProcessor<Act4PercentageEvent>
{
    private readonly IAct4Manager _act4Manager;
    private readonly IMessagePublisher<GlacernonPercentageMessage> _messagePublisher;

    public Act4PercentageEventHandler(IAct4Manager act4Manager, IMessagePublisher<GlacernonPercentageMessage> messagePublisher)
    {
        _act4Manager = act4Manager;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(Act4PercentageEvent e, CancellationToken cancellation)
    {
        Act4Status status = _act4Manager.GetStatus();

        await _messagePublisher.PublishAsync(new GlacernonPercentageMessage
        {
            AngelPercentage = status.AngelPointsPercentage,
            DemonPercentage = status.DemonPointsPercentage
        }, cancellation);
    }
}