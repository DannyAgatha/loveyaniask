using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Rewards;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Rewards.Events;

namespace WingsEmu.Plugins.GameEvents.Consumers;

public class RewardRestrictionRefreshMessageConsumer : IMessageConsumer<RewardRestrictionRefreshMessage>
{
    private readonly ISessionManager _sessionManager;

    public RewardRestrictionRefreshMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(RewardRestrictionRefreshMessage notification, CancellationToken token)
    {
        IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

        foreach (IClientSession session in sessions)
        {
            await session.EmitEventAsync(new RewardResetRestrictionEvent());
        }
    }
}