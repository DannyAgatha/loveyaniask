using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Exchange;

public class ExchangeCloseEventHandler : IAsyncEventProcessor<ExchangeCloseEvent>
{
    private readonly ISessionManager _sessionManager;

    public ExchangeCloseEventHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(ExchangeCloseEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsInExchange())
        {
            return;
        }

        PlayerExchange exchange = session.PlayerEntity.GetExchange();
        session.PlayerEntity.RemoveExchange();
        session.SendExcClosePacket(e.Type);

        IClientSession target = _sessionManager.GetSessionByCharacterId(exchange.TargetId);
        if (target == null)
        {
            return;
        }

        target.PlayerEntity.RemoveExchange();
        target.SendExcClosePacket(e.Type);
    }
}