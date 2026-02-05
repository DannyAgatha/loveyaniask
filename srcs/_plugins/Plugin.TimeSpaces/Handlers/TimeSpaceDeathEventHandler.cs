using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceTimeSpaceDeathEventHandler : IAsyncEventProcessor<TimeSpaceDeathEvent>
{
    public async Task HandleAsync(TimeSpaceDeathEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        await e.Sender.EmitEventAsync(new TimeSpaceDecreaseLiveEvent());
    }
}