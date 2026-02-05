using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.GameEvent.InstantBattle;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalInstantBattleWonEventHandler : IAsyncEventProcessor<InstantBattleWonEvent>
{
    public async Task HandleAsync(InstantBattleWonEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalInstantBattleWon++;
    }
}