using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Exchange;

public class ExchangeJoinEventHandler : IAsyncEventProcessor<ExchangeJoinEvent>
{
    private readonly IBankReputationConfiguration _bankReputationConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;

    public ExchangeJoinEventHandler(IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration, IRankingManager rankingManager)
    {
        _reputationConfiguration = reputationConfiguration;
        _bankReputationConfiguration = bankReputationConfiguration;
        _rankingManager = rankingManager;
    }

    public async Task HandleAsync(ExchangeJoinEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IClientSession target = e.Target;

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (target.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened || target.PlayerEntity.HasShopOpened)
        {
            return;
        }

        var senderExchange = new PlayerExchange(target.PlayerEntity.Id);
        var targetExchange = new PlayerExchange(session.PlayerEntity.Id);

        session.PlayerEntity.SetExchange(senderExchange);
        target.PlayerEntity.SetExchange(targetExchange);

        session.SendEmptyExchangeWindow(target.PlayerEntity.Id);
        target.SendEmptyExchangeWindow(session.PlayerEntity.Id);

        session.SendGbexPacket(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
        target.SendGbexPacket(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
    }
}