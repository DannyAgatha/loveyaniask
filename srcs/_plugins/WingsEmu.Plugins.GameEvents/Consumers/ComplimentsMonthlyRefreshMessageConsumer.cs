using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Compliments;
using WingsEmu.Game.Compliments;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class ComplimentsMonthlyRefreshMessageConsumer : IMessageConsumer<ComplimentsMonthlyRefreshMessage>
    {
        private readonly ISessionManager _sessionManager;

        public ComplimentsMonthlyRefreshMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(ComplimentsMonthlyRefreshMessage notification, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new ComplimentsMonthlyRefreshEvent
                {
                    Force = notification.Force
                });
            }
        }
    }
}