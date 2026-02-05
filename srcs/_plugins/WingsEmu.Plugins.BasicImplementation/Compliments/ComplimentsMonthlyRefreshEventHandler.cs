using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.General;

public class ComplimentsMonthlyRefreshEventHandler : IAsyncEventProcessor<ComplimentsMonthlyRefreshEvent>
{
    private readonly IComplimentsManager _complimentsManager;

    public ComplimentsMonthlyRefreshEventHandler(IComplimentsManager complimentsManager) => _complimentsManager = complimentsManager;

    public async Task HandleAsync(ComplimentsMonthlyRefreshEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        bool canRefresh = await _complimentsManager.CanRefresh(session.PlayerEntity.Id);
        if (!canRefresh && !e.Force)
        {
            return;
        }

        session.PlayerEntity.Compliment = 0;
        // TODO: Compliment rewards
    }
}