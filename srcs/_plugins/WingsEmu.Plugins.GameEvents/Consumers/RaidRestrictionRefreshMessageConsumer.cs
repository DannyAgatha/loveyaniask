using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Raid;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class RaidRestrictionRefreshMessageConsumer : IMessageConsumer<RaidRestrictionRefreshMessage>
    {
        private readonly ISessionManager _sessionManager;

        public RaidRestrictionRefreshMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(RaidRestrictionRefreshMessage notification, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new RaidResetRestrictionEvent());
            }
        }
    }
}