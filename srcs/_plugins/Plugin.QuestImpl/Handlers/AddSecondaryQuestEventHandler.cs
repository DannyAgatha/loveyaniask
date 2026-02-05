using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl.Handlers
{
    public class AddSecondaryQuestEventHandler : IAsyncEventProcessor<AddQuestEvent>
    {
        private readonly IGameLanguageService _gameLanguageService;
        private readonly INpcRunTypeQuestsConfiguration _npcRunTypeQuestsConfiguration;
        private readonly IPeriodicQuestsConfiguration _periodicQuestsConfiguration;
        private readonly IQuestFactory _questFactory;
        private readonly IQuestManager _questManager;
        private readonly IScheduler _scheduler;

        public AddSecondaryQuestEventHandler(IQuestManager questManager, IScheduler scheduler, IGameLanguageService gameLanguageService, INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration,
            IPeriodicQuestsConfiguration periodicQuestsConfiguration, IQuestFactory questFactory)
        {
            _questManager = questManager;
            _scheduler = scheduler;
            _gameLanguageService = gameLanguageService;
            _npcRunTypeQuestsConfiguration = npcRunTypeQuestsConfiguration;
            _periodicQuestsConfiguration = periodicQuestsConfiguration;
            _questFactory = questFactory;
        }

        public async Task HandleAsync(AddQuestEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            int questId = e.QuestId;

            if (e.QuestSlotType != QuestSlotType.SECONDARY)
            {
                return;
            }

            QuestDto quest = _questManager.GetQuestById(questId);
            if (quest == null)
            {
                Log.Debug($"[ERROR] Quest not found: {questId.ToString()}");
                return;
            }

            // Checks if the player has that quest active or any quest related to the same questline
            if (session.HasAlreadyQuestOrQuestline(quest, _questManager, _npcRunTypeQuestsConfiguration))
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_HAVE_QUEST, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.GetEmptyQuestSlot(e.QuestSlotType) == -1)
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_SLOT_FULL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            // Periodic quests check
            bool hasCompletedPeriodicQuest = await session.HasCompletedPeriodicQuest(quest, _questManager, _npcRunTypeQuestsConfiguration, _periodicQuestsConfiguration);
            if (_periodicQuestsConfiguration.IsDailyQuest(quest) && hasCompletedPeriodicQuest)
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_COMPLETED_PERIODIC, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (!_periodicQuestsConfiguration.IsDailyQuest(quest) && session.PlayerEntity.HasCompletedQuest(questId))
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER, $"Player {session.PlayerEntity.Name} tried to take an already completed non-daily quest ({quest.Id})");
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_COMPLETED, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            CharacterQuest characterQuest = _questFactory.NewQuest(session.PlayerEntity.Id, questId, e.QuestSlotType);
            session.PlayerEntity.AddActiveQuest(characterQuest);
            session.RefreshQuestList(_questManager, characterQuest.QuestId);

            await session.EmitEventAsync(new QuestAddedEvent
            {
                QuestId = characterQuest.QuestId,
                QuestSlotType = characterQuest.SlotType
            });

            if (characterQuest.Quest.QuestType != QuestType.COMPLETE_TIMESPACE && characterQuest.Quest.QuestType != QuestType.DROP_IN_TIMESPACE && characterQuest.Quest.QuestType != QuestType.NOTHING
                && quest.TargetMapId != 0)
            {
                session.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
                return;
            }

            // Still gotta be present for NOTHING type quests
            _scheduler.Schedule(TimeSpan.FromSeconds(1), s => session.EmitEvent(new QuestCompletedEvent(characterQuest)));
        }
    }
}