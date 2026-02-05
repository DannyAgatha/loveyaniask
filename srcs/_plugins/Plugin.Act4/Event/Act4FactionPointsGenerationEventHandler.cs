using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event;

public class Act4FactionPointsGenerationEventHandler : IAsyncEventProcessor<Act4FactionPointsGenerationEvent>
{
    private readonly Act4Configuration _act4Configuration;
    private readonly IAct4Manager _act4Manager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISessionManager _sessionManager;

    public Act4FactionPointsGenerationEventHandler(IAct4Manager act4Manager, ISessionManager sessionManager, Act4Configuration act4Configuration, IRandomGenerator randomGenerator)
    {
        _act4Manager = act4Manager;
        _sessionManager = sessionManager;
        _act4Configuration = act4Configuration;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(Act4FactionPointsGenerationEvent e, CancellationToken cancellation)
    {
        if (_act4Manager.FactionPointsLocked)
        {
            return;
        }

        int currentSessions = _sessionManager.SessionsCount;
        PointGeneration configuration = _act4Configuration.PointGeneration.FirstOrDefault(
            x => (x.PlayerAmount.Minimum == null || x.PlayerAmount.Minimum <= currentSessions) && (x.PlayerAmount.Maximum == null || currentSessions <= x.PlayerAmount.Maximum));
        if (configuration == null)
        {
            return;
        }

        _act4Manager.AddFactionPoints(_randomGenerator.RandomNumber(0, 2) == 0 ? FactionType.Angel : FactionType.Demon, configuration.PointsAmount);
    }
}