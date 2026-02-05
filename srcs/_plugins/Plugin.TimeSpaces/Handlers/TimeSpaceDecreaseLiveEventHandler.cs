using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceDecreaseLiveEventHandler : IAsyncEventProcessor<TimeSpaceDecreaseLiveEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceDecreaseLiveEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpaceDecreaseLiveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace == null)
        {
            return;
        }

        if (timeSpace.Finished)
        {
            return;
        }

        if (!timeSpace.Started)
        {
            return;
        }

        timeSpace.Instance.IncreaseOrDecreaseLives(-1);
        if (timeSpace.Instance.Lives > 0)
        {
            return;
        }

        await session.EmitEventAsync(new TimeSpaceInstanceFinishEvent(timeSpace, TimeSpaceFinishType.OUT_OF_LIVES, session.PlayerEntity.Id));
    }
}