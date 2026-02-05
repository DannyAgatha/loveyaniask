using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceAddTimeToTimerEventHandler : IAsyncEventProcessor<TimeSpaceAddTimeToTimerEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceAddTimeToTimerEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpaceAddTimeToTimerEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        TimeSpan time = e.Time;

        if (session != null && !session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = session != null ? session.PlayerEntity.TimeSpaceComponent.TimeSpace : e.TimeSpaceParty;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        if (!timeSpace.Started || timeSpace.Finished || timeSpace.Instance.InfiniteDuration)
        {
            return;
        }

        timeSpace.Instance.AddTimeToFinishDate(time);
        foreach (IClientSession member in timeSpace.Members)
        {
            member.SendTsClockPacket(timeSpace.Instance.TimeUntilEnd, true);
        }
    }
}