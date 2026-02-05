using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl.Handlers
{
    public class AddMainQuestEventHandler : IAsyncEventProcessor<AddQuestEvent>
    {
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IQuestFactory _questFactory;
        private readonly IQuestManager _questManager;
        private readonly IScheduler _scheduler;

        public AddMainQuestEventHandler(IQuestManager questManager, IQuestFactory questFactory, IScheduler scheduler, IGameLanguageService gameLanguageService)
        {
            _questManager = questManager;
            _questFactory = questFactory;
            _scheduler = scheduler;
            _gameLanguageService = gameLanguageService;
        }

        public async Task HandleAsync(AddQuestEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            int questId = e.QuestId;

            if (e.QuestSlotType != QuestSlotType.MAIN)
            {
                return;
            }

            QuestDto quest = _questManager.GetQuestById(questId);
            if (quest == null)
            {
                Log.Debug($"[ERROR] Quest not found: {questId.ToString()}");
                return;
            }

            if (session.PlayerEntity.HasQuestWithId(quest.Id))
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_HAVE_QUEST, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            // Check for possible script exploit
            if (IsPotentialMainQuestExploit(session, quest))
            {
                return;
            }

            if (session.GetEmptyQuestSlot(e.QuestSlotType) == -1)
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_SLOT_FULL, session.UserLanguage), MsgMessageType.Middle);
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

            if (characterQuest.Quest.QuestType != QuestType.COMPLETE_TIMESPACE && characterQuest.Quest.QuestType != QuestType.DROP_IN_TIMESPACE && quest.TargetMapId != 0)
            {
                session.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
            }

            // Still gotta be present for NOTHING type quests
            _scheduler.Schedule(TimeSpan.FromSeconds(1), s => session.EmitEvent(new QuestCompletedEvent(characterQuest)));
        }

        private bool IsPotentialMainQuestExploit(IClientSession session, QuestDto quest)
        {
            CompletedScriptsDto lastCompletedScript = session.PlayerEntity.GetLastCompletedScript();
            TutorialDto lastCompletedTutorial = _questManager.GetScriptTutorialByIndex(lastCompletedScript.ScriptId, lastCompletedScript.ScriptIndex);

            IReadOnlyCollection<TutorialDto> questScripts = _questManager.GetScriptsTutorialByType(TutorialActionType.START_QUEST);
            TutorialDto questScript = _questManager.GetScriptsTutorialByType(TutorialActionType.START_QUEST).FirstOrDefault(s => s.Data == quest.Id && s.Id > lastCompletedTutorial.Id);
            if (questScript == null)
            {
                return false;
            }

            if (lastCompletedTutorial != null && lastCompletedTutorial.Id > questScript.Id)
            {
                _ = session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                    $"Tried to send an already completed quest script! | ScriptId: {questScript.ScriptId}, ScriptIndex: {questScript.ScriptIndex}");
                return true;
            }

            IEnumerable<TutorialDto> mustBeCompletedScripts =
                questScripts.Where(s => s.ScriptId < questScript.ScriptId || s.ScriptId == questScript.ScriptId && s.ScriptIndex < questScript.ScriptIndex);
            foreach (TutorialDto tutorialDto in mustBeCompletedScripts)
            {
                if (session.PlayerEntity.HasCompletedScriptByIndex(tutorialDto.ScriptId, tutorialDto.ScriptIndex))
                {
                    continue;
                }

                _ = session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                    $"Tried to send a quest script without having completed previous ones! | ScriptId: {questScript.ScriptId}, ScriptIndex: {questScript.ScriptIndex} | "
                    + $"The script that was incompleted and triggered this message was: ScriptId: {tutorialDto.ScriptId} ; ScriptIndex: {tutorialDto.ScriptIndex}");
                return true; // Let's hope doesn't explode
            }

            return false;
        }
    }
}