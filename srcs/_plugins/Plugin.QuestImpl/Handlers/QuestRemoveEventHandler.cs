using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestRemoveEventHandler : IAsyncEventProcessor<QuestRemoveEvent>
    {
        private readonly IPeriodicQuestsConfiguration _periodicQuestsConfiguration;
        private readonly IQuestManager _questManager;

        public QuestRemoveEventHandler(IQuestManager questManager, IPeriodicQuestsConfiguration periodicQuestsConfiguration)
        {
            _questManager = questManager;
            _periodicQuestsConfiguration = periodicQuestsConfiguration;
        }

        public async Task HandleAsync(QuestRemoveEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;

            if (e.CharacterQuest == null)
            {
                return;
            }

            if (e.IsCompleted)
            {
                session.PlayerEntity.AddCompletedQuest(e.CharacterQuest);
                await session.EmitEventAsync(new QuestCompletedLogEvent
                {
                    CharacterQuest = e.CharacterQuest,
                    Location = e.Sender.GetLocation()
                });
            }
            else
            {
                await session.EmitEventAsync(new QuestAbandonedEvent
                {
                    QuestId = e.CharacterQuest.QuestId,
                    QuestSlotType = e.CharacterQuest.SlotType
                });
            }

            session.PlayerEntity.RemoveActiveQuest(e.CharacterQuest.QuestId);
            session.RefreshQuestList(_questManager, null);
        }
    }
}