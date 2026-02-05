using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Miniland;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Miniland.Minigames;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class MinigameRefreshProductionPointsMessageConsumer : IMessageConsumer<MinigameRefreshProductionPointsMessage>
    {
        private readonly ISessionManager _sessionManager;

        public MinigameRefreshProductionPointsMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(MinigameRefreshProductionPointsMessage msg, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new MinigameRefreshProductionEvent(msg.Force));
            }
        }
    }
}