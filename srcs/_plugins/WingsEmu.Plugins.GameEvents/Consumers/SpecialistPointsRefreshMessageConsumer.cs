using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Player;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class SpecialistPointsRefreshMessageConsumer : IMessageConsumer<SpecialistPointsRefreshMessage>
    {
        private readonly ISessionManager _sessionManager;

        public SpecialistPointsRefreshMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(SpecialistPointsRefreshMessage notification, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new SpecialistRefreshEvent
                {
                    Force = notification.Force
                });
            }
        }
    }
}