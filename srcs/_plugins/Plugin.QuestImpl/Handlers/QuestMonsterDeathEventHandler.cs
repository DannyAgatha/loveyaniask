using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestMonsterDeathEventHandler : IAsyncEventProcessor<QuestMonsterDeathEvent>
    {
        private static readonly HashSet<QuestType> _questTypes = new() { QuestType.KILL_MONSTER_BY_VNUM, QuestType.KILL_X_MOBS_SOUND_FLOWER };
        private readonly IGameLanguageService _gameLanguage;
        private readonly IQuestManager _questManager;

        public QuestMonsterDeathEventHandler(IGameLanguageService gameLanguage, IQuestManager questManager)
        {
            _gameLanguage = gameLanguage;
            _questManager = questManager;
        }

        public async Task HandleAsync(QuestMonsterDeathEvent e, CancellationToken cancellation)
        {
            IMonsterEntity monsterEntity = e.MonsterEntity;
            IClientSession session = e.Sender;

            IEnumerable<CharacterQuest> killingQuests = session.PlayerEntity.GetCurrentQuestsByTypes(_questTypes).ToArray();
            if (!killingQuests.Any())
            {
                return;
            }

            foreach (CharacterQuest characterQuest in killingQuests)
            {
                IReadOnlyCollection<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
                foreach (QuestObjectiveDto objective in objectives)
                {
                    if (objective.Data0 != monsterEntity.MonsterVNum && characterQuest.Quest.QuestType != QuestType.KILL_X_MOBS_SOUND_FLOWER)
                    {
                        continue;
                    }

                    if (characterQuest.Quest.QuestType == QuestType.KILL_X_MOBS_SOUND_FLOWER)
                    {
                        if (session.PlayerEntity.Level - monsterEntity.Level > 10)
                        {
                            continue;
                        }
                    }

                    CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                    if (questObjectiveDto.CurrentAmount < questObjectiveDto.RequiredAmount)
                    {
                        questObjectiveDto.CurrentAmount++;
                        await session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        if (characterQuest.Quest.QuestType != QuestType.KILL_X_MOBS_SOUND_FLOWER)
                        {
                            string monsterName = _gameLanguage.GetLanguage(GameDataType.NpcMonster, monsterEntity.Name, session.UserLanguage);
                            session.SendChatMessage(string.Format(_gameLanguage
                                    .GetLanguage(GameDialogKey.QUEST_CHATMESSAGE_X_HUNTING_Y_Z, session.UserLanguage), monsterName, questObjectiveDto.CurrentAmount, questObjectiveDto.RequiredAmount),
                                ChatMessageColorType.Red);
                        }
                    }

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