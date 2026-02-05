using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl
{
    [Name("Quests")]
    [Group("quests", "quest")]
    [Description("Module related to Quest commands.")]
    [RequireAuthority(AuthorityType.SGM)]
    public class QuestModule : SaltyModuleBase
    {
        private readonly IQuestFactory _questFactory;
        private readonly IQuestManager _questManager;

        public QuestModule(IQuestManager questManager, IQuestFactory questFactory)
        {
            _questManager = questManager;
            _questFactory = questFactory;
        }

        [Command("add")]
        [Description("Add the quest linked to a given ID")]
        public async Task<SaltyCommandResult> AddQuest(int questId, string questSlotTypeName)
        {
            if (!Enum.TryParse(questSlotTypeName, out QuestSlotType questSlotType))
            {
                return new SaltyCommandResult(false, "Wrong quest slot type");
            }

            QuestDto quest = _questManager.GetQuestById(questId);
            if (quest == null)
            {
                return new SaltyCommandResult(false, "Quest null");
            }

            Context.Player.PlayerEntity.AddActiveQuest(_questFactory.NewQuest(Context.Player.PlayerEntity.Id, questId, questSlotType));
            Context.Player.RefreshQuestList(_questManager, quest.Id);
            Context.Player.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
            return new SaltyCommandResult(true, "Quest added!");
        }

        [Command("give")]
        [Description("Gives the quest to a character on a slot (general, main, secondary)")]
        public async Task<SaltyCommandResult> AddQuest(IClientSession session, int questId, string questSlotTypeName)
        {
            if (!Enum.TryParse(questSlotTypeName, out QuestSlotType questSlotType))
            {
                return new SaltyCommandResult(false, "Wrong quest slot type");
            }

            QuestDto quest = _questManager.GetQuestById(questId);
            if (quest == null)
            {
                return new SaltyCommandResult(false, "Quest null");
            }

            if (session == null)
            {
                return new SaltyCommandResult(false, "Player null");
            }

            session.PlayerEntity.AddActiveQuest(_questFactory.NewQuest(session.PlayerEntity.Id, questId, questSlotType));
            session.RefreshQuestList(_questManager, quest.Id);
            session.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
            return new SaltyCommandResult(true, "Quest added!");
        }

        [Command("getscripts")]
        [Description("Get the scripts completed by the player")]
        public async Task<SaltyCommandResult> GetPlayerCompletedScripts(IClientSession session)
        {
            if (session == null)
            {
                return new SaltyCommandResult(false, "Player null");
            }

            foreach (CompletedScriptsDto completedScriptsDto in session.PlayerEntity.GetCompletedScripts())
            {
                Context.Player.SendChatMessage($"script {completedScriptsDto.ScriptId} {completedScriptsDto.ScriptIndex}", ChatMessageColorType.Yellow);
            }

            return new SaltyCommandResult(true);
        }

        [Command("completescripts", "cs")]
        [Description("Forces the script completion, removing any MAIN quest running and any completed script after the specified, and sends the next one.")]
        public async Task<SaltyCommandResult> ForceCompleteScripts(IClientSession session, int scriptId, int scriptIndex)
        {
            if (session == null)
            {
                return new SaltyCommandResult(false, "Player null");
            }

            TutorialDto tutorialDto = _questManager.GetScriptTutorialByIndex(scriptId, scriptIndex);
            if (tutorialDto == null)
            {
                return new SaltyCommandResult(false, "Script null");
            }

            IReadOnlyCollection<TutorialDto> scripts = _questManager.GetScriptsTutorialUntilIndex(scriptId, scriptIndex);
            IEnumerable<CompletedScriptsDto> playerCompletedScripts = session.PlayerEntity.GetCompletedScripts();

            foreach (TutorialDto script in scripts)
            {
                session.PlayerEntity.SaveScript(script.ScriptId, script.ScriptIndex, script.Type, DateTime.UtcNow);
                if (script.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION)
                {
                    session.PlayerEntity.AddCompletedQuest(_questFactory.NewQuest(session.PlayerEntity.Id, script.Data, QuestSlotType.MAIN));
                }
            }

            foreach (CompletedScriptsDto completedScript in playerCompletedScripts)
            {
                if (completedScript.ScriptId < scriptId || completedScript.ScriptId == scriptId && completedScript.ScriptIndex <= scriptIndex)
                {
                    continue;
                }

                session.PlayerEntity.RemoveCompletedScript(completedScript.ScriptId, completedScript.ScriptIndex);

                TutorialDto script = _questManager.GetScriptTutorialByIndex(completedScript.ScriptId, completedScript.ScriptIndex);
                if (script.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION)
                {
                    session.PlayerEntity.RemoveCompletedQuest(script.Data);
                }
            }

            TutorialDto nextScript = _questManager.GetScriptTutorialById(scripts.Last().Id + 1);
            if (nextScript == null)
            {
                return new SaltyCommandResult(true, "That was the last script,");
            }

            CharacterQuest characterQuest = session.GetQuestBySlot(5);
            if (characterQuest != null) // Remove a MAIN quest if there is any
            {
                session.DeleteQuestTarget(characterQuest);
                session.PlayerEntity.RemoveActiveQuest(characterQuest.QuestId);
                session.RefreshQuestList(_questManager, null);
            }

            if (nextScript.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION && session.PlayerEntity.HasQuestWithId(nextScript.Data))
            {
                QuestDto quest = _questManager.GetQuestById(nextScript.Data);
                session.PlayerEntity.AddActiveQuest(_questFactory.NewQuest(session.PlayerEntity.Id, nextScript.Data, QuestSlotType.MAIN));
                session.RefreshQuestList(_questManager, quest.Id);
                session.SendTargetQuest(quest.TargetMapX, quest.TargetMapY, quest.TargetMapId, quest.Id);
            }

            session.SendScriptPacket(nextScript.ScriptId, nextScript.ScriptIndex);

            return new SaltyCommandResult(true, "Scripts completed");
        }

        [Command("remove")]
        [Description("Remove the quest linked to a given ID")]
        public async Task<SaltyCommandResult> RemoveQuest(int questId)
        {
            CharacterQuest quest = Context.Player.PlayerEntity.GetQuestById(questId);
            if (quest == null)
            {
                return new SaltyCommandResult(false, "Quest null");
            }

            Context.Player.DeleteQuestTarget(quest);
            Context.Player.PlayerEntity.RemoveActiveQuest(quest.QuestId);
            Context.Player.RefreshQuestList(_questManager, null);
            return new SaltyCommandResult(true, "Quest removed!");
        }

        [Command("remove")]
        [Description("Remove the quest linked to a given ID of a character")]
        public async Task<SaltyCommandResult> RemoveQuest(IClientSession session, int questId)
        {
            if (session == null)
            {
                return new SaltyCommandResult(false, "Player null");
            }

            CharacterQuest quest = session.PlayerEntity.GetQuestById(questId);
            if (quest == null)
            {
                return new SaltyCommandResult(false, "Quest null");
            }

            session.DeleteQuestTarget(quest);
            session.PlayerEntity.RemoveActiveQuest(quest.QuestId);
            session.RefreshQuestList(_questManager, null);
            return new SaltyCommandResult(true, "Quest removed!");
        }

        [Command("showinfo")]
        [Description("Show info related to a player quests")]
        public async Task<SaltyCommandResult> ShowInfo(IClientSession session)
        {
            if (session == null)
            {
                return new SaltyCommandResult(false, "Player null");
            }

            Context.Player.SendChatMessage($"== Active quests from {session.PlayerEntity.Name} ==", ChatMessageColorType.Yellow);
            foreach (CharacterQuest quest in session.PlayerEntity.GetCurrentQuests())
            {
                Context.Player.SendChatMessage($"- Quest VNUM: {quest.QuestId}; Slot: {quest.SlotType}; Type: {quest.Quest.QuestType}", ChatMessageColorType.Yellow);
                foreach (QuestObjectiveDto objective in quest.Quest.Objectives)
                {
                    CharacterQuestObjectiveDto questObjectiveDto = quest.ObjectiveAmount[objective.ObjectiveIndex];
                    Context.Player.SendChatMessage($"ObjectiveIndex: {objective.ObjectiveIndex}; CompletedAmount: {questObjectiveDto.CurrentAmount}", ChatMessageColorType.Yellow);
                }
            }

            return new SaltyCommandResult(true, "Scripts showed");
        }

        [Command("showinfo")]
        [Description("Show info related to your quests")]
        public async Task<SaltyCommandResult> ShowInfo()
        {
            Context.Player.SendChatMessage($"== Active quests from {Context.Player.PlayerEntity.Name} ==", ChatMessageColorType.Yellow);
            foreach (CharacterQuest quest in Context.Player.PlayerEntity.GetCurrentQuests())
            {
                Context.Player.SendChatMessage($"- Quest VNUM: {quest.QuestId}; Slot: {quest.SlotType}; Type: {quest.Quest.QuestType}", ChatMessageColorType.Yellow);
                foreach (QuestObjectiveDto objective in quest.Quest.Objectives)
                {
                    CharacterQuestObjectiveDto questObjectiveDto = quest.ObjectiveAmount[objective.ObjectiveIndex];
                    Context.Player.SendChatMessage($"ObjectiveIndex: {objective.ObjectiveIndex}; CompletedAmount: {questObjectiveDto.CurrentAmount}", ChatMessageColorType.Yellow);
                }
            }

            return new SaltyCommandResult(true);
        }

        [Command("clear")]
        [Description("Clear all quests")]
        public async Task<SaltyCommandResult> ClearAllQuests()
        {
            foreach (CharacterQuest quest in Context.Player.PlayerEntity.GetCurrentQuests())
            {
                Context.Player.PlayerEntity.RemoveActiveQuest(quest.QuestId);
                Context.Player.DeleteQuestTarget(quest);
            }

            Context.Player.RefreshQuestList(_questManager, null);
            return new SaltyCommandResult(true);
        }

        [Command("refresh-dq", "refreshdailyquests", "rdq")]
        [Description("Refresh your daily minigame points.")]
        public async Task<SaltyCommandResult> RefreshDailyQuests([Description("Force refresh")] bool force = false)
        {
            Context.Player.EmitEvent(new QuestDailyRefreshEvent { Force = force });
            return new SaltyCommandResult(true, "Daily quests refreshed!");
        }

        [Command("set-soundflowers")]
        [Description("Gives the player X amount of unstarted soundflower quests")]
        public async Task<SaltyCommandResult> GiveSoundFlowers(IClientSession session, int amount)
        {
            IReadOnlyCollection<CharacterQuest> generalQuests = session.PlayerEntity.GetCurrentQuests().Where(s => s.SlotType == QuestSlotType.MAIN).ToList();
            if (generalQuests.Count + amount > 5)
            {
                return new SaltyCommandResult(false, "You can't have more than 5 GENERAL quests active!");
            }

            int currentSoundFlowers = session.PlayerEntity.GetPendingSoundFlowerQuests();
            for (int i = 0; i < currentSoundFlowers; i++)
            {
                session.PlayerEntity.DecreasePendingSoundFlowerQuests();
            }

            for (int i = 0; i < amount; i++)
            {
                session.PlayerEntity.IncreasePendingSoundFlowerQuests();
            }

            session.RefreshQuestList(_questManager, null);
            return new SaltyCommandResult(true, "Sound flower quests added!");
        }
    }
}