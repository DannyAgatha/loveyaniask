using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestHarvestEventHandler : IAsyncEventProcessor<QuestHarvestEvent>
    {
        private readonly IQuestManager _questManager;

        public QuestHarvestEventHandler(IQuestManager questManager) => _questManager = questManager;

        public async Task HandleAsync(QuestHarvestEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IReadOnlyCollection<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuestsByType(QuestType.COLLECT).ToList();
            if (!characterQuests.Any())
            {
                return;
            }

            foreach (CharacterQuest characterQuest in characterQuests)
            {
                IReadOnlyCollection<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
                foreach (QuestObjectiveDto objective in objectives)
                {
                    CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                    if (e.ItemVnum != objective.Data1 || e.NpcVnum != objective.Data0
                        || questObjectiveDto.CurrentAmount >= questObjectiveDto.RequiredAmount)
                    {
                        continue;
                    }

                    questObjectiveDto.CurrentAmount++;
                    await session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                    {
                        CharacterQuest = characterQuest
                    });

                    if (session.PlayerEntity.IsQuestCompleted(characterQuest))
                    {
                        await session.EmitEventAsync(new QuestCompletedEvent(characterQuest));
                    }
                    else
                    {
                        session.RefreshQuestProgress(_questManager, characterQuest.QuestId);
                    }
                }
            }
        }
    }
}