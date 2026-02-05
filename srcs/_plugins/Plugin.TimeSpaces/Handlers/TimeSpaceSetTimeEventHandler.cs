using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceSetTimeEventHandler : IAsyncEventProcessor<TimeSpaceSetTimeEvent>
{
    public async Task HandleAsync(TimeSpaceSetTimeEvent e, CancellationToken cancellation)
    {
        TimeSpaceParty timeSpace = e.TimeSpaceParty;
        TimeSpan time = e.Time;

        if (timeSpace?.Instance == null)
        {
            return;
        }

        if (timeSpace.Finished || !timeSpace.Started)
        {
            return;
        }

        timeSpace.Instance.UpdateFinishDate(time);
        timeSpace.Instance.InfiniteDuration = false;
        foreach (IClientSession member in timeSpace.Members)
        {
            member.SendTsClockPacket(timeSpace.Instance.TimeUntilEnd, true);
        }
    }
}