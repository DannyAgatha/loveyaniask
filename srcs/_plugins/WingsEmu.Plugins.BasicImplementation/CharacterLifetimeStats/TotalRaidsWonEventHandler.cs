using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalRaidsWonEventHandler : IAsyncEventProcessor<RaidWonEvent>
{
    public async Task HandleAsync(RaidWonEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalRaidsWon++;
    }
}