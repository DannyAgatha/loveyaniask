using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestDailyRefreshEventHandler : IAsyncEventProcessor<QuestDailyRefreshEvent>
    {
        private readonly IQuestManager _questManager;

        public QuestDailyRefreshEventHandler(IQuestManager questManager) => _questManager = questManager;

        public async Task HandleAsync(QuestDailyRefreshEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            bool canRefresh = await _questManager.CanRefreshDailyQuests(session.PlayerEntity.Id);

            if (canRefresh == false && e.Force == false)
            {
                session.SendDebugMessage("Daily quests already refreshed for today");
                return;
            }

            session.PlayerEntity.ClearCompletedPeriodicQuests();
        }
    }
}