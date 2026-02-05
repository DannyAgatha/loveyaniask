using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Quests;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class QuestDailyRefreshMessageConsumer : IMessageConsumer<QuestDailyRefreshMessage>
    {
        private readonly ISessionManager _sessionManager;
        public QuestDailyRefreshMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(QuestDailyRefreshMessage notification, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new QuestDailyRefreshEvent
                {
                    Force = notification.Force
                });
            }
        }
    }
}