using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceIncreaseScoreEventHandler : IAsyncEventProcessor<TimeSpaceIncreaseScoreEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceIncreaseScoreEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpaceIncreaseScoreEvent e, CancellationToken cancellation)
    {
        int amountToIncrease = e.AmountToIncrease;

        TimeSpaceParty timeSpaceParty = null;
        if (e.TimeSpaceParty != null)
        {
            timeSpaceParty = e.TimeSpaceParty;
        }

        if (timeSpaceParty == null)
        {
            return;
        }

        if (!timeSpaceParty.Started || timeSpaceParty.Finished)
        {
            return;
        }

        timeSpaceParty.Instance.IncreaseScoreByAmount(amountToIncrease);
        foreach (IClientSession member in timeSpaceParty.Members)
        {
            member.RefreshTimespaceScoreUi();
        }
    }
}