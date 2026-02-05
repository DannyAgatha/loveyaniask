using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestEnemyDeathEventHandler : IAsyncEventProcessor<QuestEnemyDeathEvent>
    {
        private static readonly HashSet<QuestType> QuestTypes = [QuestType.KILL_PLAYER_IN_REGION];
        private readonly IQuestManager _questManager;

        
        
        public QuestEnemyDeathEventHandler(IQuestManager questManager)
        {
            _questManager = questManager;
        }

        public async Task HandleAsync(QuestEnemyDeathEvent e, CancellationToken cancellation)
        {
            IPlayerEntity killedPlayer = e.KilledPlayer;
            IClientSession killerSession = e.KillerSession;

            CharacterQuest[] killingQuests = killerSession.PlayerEntity.GetCurrentQuestsByTypes(QuestTypes).ToArray();
            if (killingQuests.Length == 0)
            {
                return;
            }

            foreach (CharacterQuest characterQuest in killingQuests)
            {
                foreach (CharacterQuestObjectiveDto questObjectiveDto in characterQuest.Quest.Objectives.Select(objective => characterQuest.ObjectiveAmount[objective.ObjectiveIndex]))
                {
                    if (questObjectiveDto.CurrentAmount < questObjectiveDto.RequiredAmount)
                    {
                        questObjectiveDto.CurrentAmount++;
                        
                        await killerSession.EmitEventAsync(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        killerSession.SendChatMessage(string.Format(
                                killerSession.GetLanguage(GameDialogKey.QUEST_CHATMESSAGE_X_KILLING_Y_Z), questObjectiveDto.CurrentAmount, questObjectiveDto.RequiredAmount),
                            ChatMessageColorType.Red);
                    }

                    if (killerSession.PlayerEntity.IsQuestCompleted(characterQuest))
                    {
                        await killerSession.EmitEventAsync(new QuestCompletedEvent(characterQuest));
                    }
                    else
                    {
                        killerSession.RefreshQuestProgress(_questManager, characterQuest.QuestId);
                    }
                }
            }
        }
    }
}
