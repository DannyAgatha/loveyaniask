using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestItemPickUpEventHandler : IAsyncEventProcessor<QuestItemPickUpEvent>
    {
        private static readonly QuestType[] DropQuests = { QuestType.DROP_CHANCE, QuestType.DROP_CHANCE_2, QuestType.DROP_HARDCODED, QuestType.DROP_IN_TIMESPACE };
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IItemsManager _itemsManager;
        private readonly IQuestManager _questManager;

        public QuestItemPickUpEventHandler(IGameLanguageService gameLanguageService, IItemsManager itemsManager, IQuestManager questManager)
        {
            _gameLanguageService = gameLanguageService;
            _itemsManager = itemsManager;
            _questManager = questManager;
        }

        public async Task HandleAsync(QuestItemPickUpEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IEnumerable<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuestsByTypes(DropQuests).ToArray();
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
                    if (e.ItemVnum != (characterQuest.Quest.QuestType == QuestType.DROP_HARDCODED ? objective.Data0 : objective.Data1))
                    {
                        continue;
                    }

                    int amountLeft = questObjectiveDto.RequiredAmount - questObjectiveDto.CurrentAmount;
                    if (amountLeft == 0)
                    {
                        break;
                    }

                    int questObjective = Math.Min(amountLeft, e.Amount);
                    questObjectiveDto.CurrentAmount += questObjective;

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

                    if (!e.SendMessage)
                    {
                        continue;
                    }

                    string itemName = _itemsManager.GetItem(e.ItemVnum).GetItemName(_gameLanguageService, session.UserLanguage);
                    session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.QUEST_CHATMESSAGE_ITEM_PICK_UP, itemName,
                        questObjectiveDto.CurrentAmount, questObjectiveDto.RequiredAmount), ChatMessageColorType.Red);
                }
            }
        }
    }
}