using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceStartClockEventHandler : IAsyncEventProcessor<TimeSpaceStartClockEvent>
{
    public async Task HandleAsync(TimeSpaceStartClockEvent e, CancellationToken cancellation)
    {
        TimeSpaceParty timeSpaceParty = e.TimeSpaceParty;

        if (timeSpaceParty?.Instance == null)
        {
            return;
        }

        if (timeSpaceParty.Instance.InfiniteDuration)
        {
            return;
        }

        foreach (IClientSession session in timeSpaceParty.Members)
        {
            if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
            {
                continue;
            }

            session.SendTsClockPacket(timeSpaceParty.Instance.TimeUntilEnd, e.IsVisible);
        }
    }
}